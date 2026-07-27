using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;
using SalgaFacil.Tests.TestSupport;
using SalgaFacil.Web.Contracts.Pagamentos;
using SalgaFacil.Web.Services;
using Xunit;

namespace SalgaFacil.Tests.Web;

/// <summary>
/// PIX-MANUAL-002: autorização (isolamento multi-tenant e "dono do pedido"), transições de
/// status do comprovante e confirmação idempotente do recebimento — as quatro áreas de risco
/// mais sensíveis apontadas no contrato ("proteger contra troca de identificador, upload
/// malicioso, confirmação duplicada" + isolamento multi-tenant preservado).
///
/// Usa Sqlite em memória (via <see cref="SqliteContexto"/>) em vez de mocks para o DbContext:
/// PagamentoPixService faz Include/consultas reais via EF Core, e um mock de DbSet não exercitaria
/// isso com fidelidade.
/// </summary>
public sealed class PagamentoPixServiceTests : IDisposable
{
    private readonly string _pastaComprovantes;

    public PagamentoPixServiceTests()
    {
        _pastaComprovantes = Directory.CreateTempSubdirectory("salgafacil-pix-tests-").FullName;
    }

    public void Dispose()
    {
        try { Directory.Delete(_pastaComprovantes, recursive: true); }
        catch { /* best-effort */ }
    }

    private ComprovanteArmazenamentoService CriarArmazenamento() =>
        new(new FakeWebHostEnvironment { ContentRootPath = _pastaComprovantes }, NullLogger<ComprovanteArmazenamentoService>.Instance);

    private PagamentoPixService CriarServico(SalgaFacilDbContext db, int? empresaIdAdmin = null) =>
        new(db, new FakeEmpresaContext(empresaIdAdmin), CriarArmazenamento(), NullLogger<PagamentoPixService>.Instance);

    /// <summary>Cenário padrão: uma empresa com Pix ativo, um cliente dono de um pedido Pix em Aguardando, e um segundo cliente/empresa para os testes de autorização (IDOR).</summary>
    private static async Task<Cenario> SemearAsync(SalgaFacilDbContext db, decimal total = 25.00m)
    {
        var empresa = new Empresa
        {
            Slug = "loja-teste", Nome = "Loja Teste", NomeFantasia = "Loja Teste",
            PixAtivo = true, PixChave = "loja@teste.com.br", PixNomeBeneficiario = "Loja Teste LTDA"
        };
        var outraEmpresa = new Empresa { Slug = "loja-rival", Nome = "Loja Rival", PixAtivo = true, PixChave = "rival@teste.com.br", PixNomeBeneficiario = "Rival" };
        db.Empresas.AddRange(empresa, outraEmpresa);
        await db.SaveChangesAsync();

        var clienteDono = new Cliente { EmpresaId = empresa.Id, Nome = "Cliente Dono", Telefone = "11999990001" };
        var outroClienteMesmaEmpresa = new Cliente { EmpresaId = empresa.Id, Nome = "Outro Cliente", Telefone = "11999990002" };
        var clienteOutraEmpresa = new Cliente { EmpresaId = outraEmpresa.Id, Nome = "Cliente Rival", Telefone = "11999990003" };
        db.Clientes.AddRange(clienteDono, outroClienteMesmaEmpresa, clienteOutraEmpresa);

        var usuario1 = new Usuario { EmpresaId = empresa.Id, Nome = "Funcionaria Ana", Email = "ana@teste.com.br", SenhaHash = "x" };
        var usuario2 = new Usuario { EmpresaId = empresa.Id, Nome = "Funcionario Bruno", Email = "bruno@teste.com.br", SenhaHash = "x" };
        db.Usuarios.AddRange(usuario1, usuario2);
        await db.SaveChangesAsync();

        var pedido = new Pedido
        {
            EmpresaId = empresa.Id,
            ClienteId = clienteDono.Id,
            Total = total,
            FormaPagamento = FormaPagamento.Pix,
            StatusPagamento = StatusPagamento.Aguardando
        };
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        return new Cenario(empresa, outraEmpresa, clienteDono, outroClienteMesmaEmpresa, clienteOutraEmpresa, usuario1, usuario2, pedido);
    }

    private sealed record Cenario(
        Empresa Empresa, Empresa OutraEmpresa,
        Cliente ClienteDono, Cliente OutroClienteMesmaEmpresa, Cliente ClienteOutraEmpresa,
        Usuario Usuario1, Usuario Usuario2, Pedido Pedido);

    // ---------- Autorização ----------

    [Fact]
    public async Task ObterParaClienteAsync_DonoDoPedido_RetornaDto()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db);

        var dto = await servico.ObterParaClienteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id);

        Assert.NotNull(dto);
        Assert.Equal(cenario.Pedido.Id, dto!.PedidoId);
        Assert.Equal(StatusPagamentoPix.Aguardando, dto.Status);
    }

    [Fact]
    public async Task ObterParaClienteAsync_OutroClienteDaMesmaEmpresa_RetornaNull()
    {
        // Tentativa de "troca de identificador": um cliente autenticado tentando ver o pedido de
        // outro cliente da MESMA empresa (não é nem um problema de multi-tenant, é IDOR direto).
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db);

        var dto = await servico.ObterParaClienteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.OutroClienteMesmaEmpresa.Id);

        Assert.Null(dto);
    }

    [Fact]
    public async Task ObterParaClienteAsync_ClienteDeOutroTenant_RetornaNull()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db);

        var dto = await servico.ObterParaClienteAsync(cenario.OutraEmpresa.Id, cenario.Pedido.Id, cenario.ClienteOutraEmpresa.Id);

        Assert.Null(dto);
    }

    [Fact]
    public async Task ObterParaAdministracaoAsync_PedidoDeOutraEmpresa_RetornaNull()
    {
        // Isolamento multi-tenant: o painel administrativo da empresa rival não pode enxergar o
        // pedido da primeira empresa, mesmo sabendo o Id numérico do pedido.
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.OutraEmpresa.Id);

        var dto = await servico.ObterParaAdministracaoAsync(cenario.Pedido.Id);

        Assert.Null(dto);
    }

    [Fact]
    public async Task EnviarComprovanteAsync_ClienteNaoDonoTentandoTrocarIdentificador_Lanca()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.EnviarComprovanteAsync(
                cenario.Empresa.Id, cenario.Pedido.Id, cenario.OutroClienteMesmaEmpresa.Id,
                ArquivosDeTeste.JpgValido(), "comprovante.jpg", "image/jpeg"));
    }

    // ---------- Upload / transições de status ----------

    [Fact]
    public async Task EnviarComprovanteAsync_ArquivoValido_TransicionaParaComprovanteEnviado()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db);

        var dto = await servico.EnviarComprovanteAsync(
            cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id,
            ArquivosDeTeste.JpgValido(), "comprovante.jpg", "image/jpeg");

        Assert.Equal(StatusPagamentoPix.ComprovanteEnviado, dto.Status);
        Assert.True(dto.TemComprovante);
        Assert.Equal("comprovante.jpg", dto.ComprovanteNomeArquivo);
    }

    [Fact]
    public async Task EnviarComprovanteAsync_ArquivoDisfarcado_LancaENaoAlteraStatus()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.EnviarComprovanteAsync(
                cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id,
                ArquivosDeTeste.JpgFalsificado(), "comprovante.jpg", "image/jpeg"));

        var dto = await servico.ObterParaClienteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id);
        Assert.Equal(StatusPagamentoPix.Aguardando, dto!.Status);
        Assert.False(dto.TemComprovante);
    }

    [Fact]
    public async Task EnviarComprovanteAsync_PedidoJaPago_Lanca()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);
        await servico.ConfirmarRecebimentoAsync(cenario.Pedido.Id, cenario.Usuario1.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.EnviarComprovanteAsync(
                cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id,
                ArquivosDeTeste.JpgValido(), "comprovante.jpg", "image/jpeg"));
    }

    [Fact]
    public async Task EnviarComprovanteAsync_AposRejeicao_PermiteReenvioETransicionaDeVoltaParaComprovanteEnviado()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servicoCliente = CriarServico(ctx.Db);
        var servicoAdmin = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);

        await servicoCliente.EnviarComprovanteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id, ArquivosDeTeste.JpgValido(), "primeiro.jpg", "image/jpeg");
        await servicoAdmin.RejeitarComprovanteAsync(cenario.Pedido.Id, cenario.Usuario1.Id, "Valor não confere com o extrato.");

        var reenviado = await servicoCliente.EnviarComprovanteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id, ArquivosDeTeste.PngValido(), "segundo.png", "image/png");

        Assert.Equal(StatusPagamentoPix.ComprovanteEnviado, reenviado.Status);
        Assert.Null(reenviado.ComprovanteMotivoRejeicao); // motivo antigo é limpo no reenvio
        Assert.Equal("segundo.png", reenviado.ComprovanteNomeArquivo);
    }

    [Fact]
    public async Task MarcarEmAnaliseAsync_ApartirDeComprovanteEnviado_Transiciona()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);
        await servico.EnviarComprovanteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id, ArquivosDeTeste.JpgValido(), "c.jpg", "image/jpeg");

        await servico.MarcarEmAnaliseAsync(cenario.Pedido.Id, cenario.Usuario1.Id);

        var dto = await servico.ObterParaAdministracaoAsync(cenario.Pedido.Id);
        Assert.Equal(StatusPagamentoPix.EmAnalise, dto!.Status);
    }

    [Fact]
    public async Task MarcarEmAnaliseAsync_QuandoNaoEstaEmComprovanteEnviado_NaoAlteraStatus()
    {
        // Dois funcionários clicando quase ao mesmo tempo não deve virar erro — é um no-op silencioso.
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);

        var ex = await Record.ExceptionAsync(() => servico.MarcarEmAnaliseAsync(cenario.Pedido.Id, cenario.Usuario1.Id));

        Assert.Null(ex);
        var dto = await servico.ObterParaAdministracaoAsync(cenario.Pedido.Id);
        Assert.Equal(StatusPagamentoPix.Aguardando, dto!.Status);
    }

    [Fact]
    public async Task RejeitarComprovanteAsync_SemMotivo_Lanca()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);
        await servico.EnviarComprovanteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id, ArquivosDeTeste.JpgValido(), "c.jpg", "image/jpeg");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.RejeitarComprovanteAsync(cenario.Pedido.Id, cenario.Usuario1.Id, "   "));
    }

    [Fact]
    public async Task RejeitarComprovanteAsync_Valido_RegistraMotivoERevisorEClienteVeMotivo()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servicoCliente = CriarServico(ctx.Db);
        var servicoAdmin = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);
        await servicoCliente.EnviarComprovanteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id, ArquivosDeTeste.JpgValido(), "c.jpg", "image/jpeg");

        await servicoAdmin.RejeitarComprovanteAsync(cenario.Pedido.Id, cenario.Usuario1.Id, "Comprovante ilegível.");

        var visaoCliente = await servicoCliente.ObterParaClienteAsync(cenario.Empresa.Id, cenario.Pedido.Id, cenario.ClienteDono.Id);
        Assert.Equal(StatusPagamentoPix.Rejeitado, visaoCliente!.Status);
        Assert.Equal("Comprovante ilegível.", visaoCliente.ComprovanteMotivoRejeicao);
        Assert.True(visaoCliente.PodeEnviarComprovante); // pode reenviar após rejeição
    }

    [Fact]
    public async Task RejeitarComprovanteAsync_PedidoJaPago_Lanca()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);
        await servico.ConfirmarRecebimentoAsync(cenario.Pedido.Id, cenario.Usuario1.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.RejeitarComprovanteAsync(cenario.Pedido.Id, cenario.Usuario1.Id, "motivo qualquer"));
    }

    // ---------- Confirmação idempotente ----------

    [Fact]
    public async Task ConfirmarRecebimentoAsync_NaoExigeComprovanteEnviado_ConfirmaDiretoDeAguardando()
    {
        // Regra de negócio preservada do contrato original: quem confirma é a loja, checando a
        // própria conta — o comprovante é só um apoio, nunca um pré-requisito da confirmação.
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);

        await servico.ConfirmarRecebimentoAsync(cenario.Pedido.Id, cenario.Usuario1.Id);

        var dto = await servico.ObterParaAdministracaoAsync(cenario.Pedido.Id);
        Assert.Equal(StatusPagamentoPix.Pago, dto!.Status);
    }

    [Fact]
    public async Task ConfirmarRecebimentoAsync_ChamadoDuasVezes_NaoSobrescreveDataNemUsuarioDaPrimeiraConfirmacao()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);

        await servico.ConfirmarRecebimentoAsync(cenario.Pedido.Id, cenario.Usuario1.Id);
        var pedidoAposPrimeira = await ctx.Db.Pedidos.FindAsync(cenario.Pedido.Id);
        var dataDaPrimeiraConfirmacao = pedidoAposPrimeira!.PagamentoConfirmadoEm;

        // Segunda confirmação, por um usuário DIFERENTE — simula duplo clique/duas abas abertas.
        await servico.ConfirmarRecebimentoAsync(cenario.Pedido.Id, cenario.Usuario2.Id);

        var pedidoFinal = await ctx.Db.Pedidos.AsNoTracking().FirstAsync(p => p.Id == cenario.Pedido.Id);
        Assert.Equal(StatusPagamento.Pago, pedidoFinal.StatusPagamento);
        Assert.Equal(cenario.Usuario1.Id, pedidoFinal.PagamentoConfirmadoPorUsuarioId);
        Assert.Equal(dataDaPrimeiraConfirmacao, pedidoFinal.PagamentoConfirmadoEm);
    }

    [Fact]
    public async Task ConfirmarRecebimentoAsync_RegistraDataHoraEUsuarioResponsavel()
    {
        using var ctx = new SqliteContexto();
        var cenario = await SemearAsync(ctx.Db);
        var servico = CriarServico(ctx.Db, empresaIdAdmin: cenario.Empresa.Id);
        var antes = DateTime.UtcNow;

        await servico.ConfirmarRecebimentoAsync(cenario.Pedido.Id, cenario.Usuario2.Id);

        var pedido = await ctx.Db.Pedidos.AsNoTracking().FirstAsync(p => p.Id == cenario.Pedido.Id);
        Assert.Equal(cenario.Usuario2.Id, pedido.PagamentoConfirmadoPorUsuarioId);
        Assert.NotNull(pedido.PagamentoConfirmadoEm);
        Assert.True(pedido.PagamentoConfirmadoEm >= antes);
    }
}
