using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Domain.Services;
using SalgaFacil.Infrastructure.Data;
using SalgaFacil.Web.Contracts.Pagamentos;

namespace SalgaFacil.Web.Services;

/// <summary>
/// Implementação real de <see cref="IPagamentoPixService"/> (PIX-MANUAL-001/002). Sem gateway,
/// webhook, cartão ou credencial bancária: o cliente paga direto no app do banco dele (usando o
/// QR Code ou o Pix Copia e Cola gerados aqui), anexa o comprovante, e o comerciante autenticado
/// confirma manualmente o recebimento depois de checar o extrato/app do banco da loja.
/// </summary>
public sealed class PagamentoPixService(
    SalgaFacilDbContext db,
    IEmpresaContext empresaContext,
    ComprovanteArmazenamentoService comprovantes,
    ILogger<PagamentoPixService> logger) : IPagamentoPixService
{
    private const string MensagemIndisponivel = "Pagamento Pix indisponível no momento. Entre em contato com a loja.";

    public async Task<ConfiguracaoPixDto> ObterConfiguracaoAsync()
    {
        var empresa = await ObterEmpresaAutenticadaAsync();
        return new ConfiguracaoPixDto
        {
            Ativo = empresa.PixAtivo,
            Chave = empresa.PixChave,
            NomeBeneficiario = empresa.PixNomeBeneficiario,
            Simulado = false
        };
    }

    public async Task SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao)
    {
        var empresa = await ObterEmpresaAutenticadaAsync();
        var chave = configuracao.Chave?.Trim();
        var nomeBeneficiario = configuracao.NomeBeneficiario?.Trim();

        if (chave is { Length: > 140 })
            throw new InvalidOperationException("A chave Pix deve ter no máximo 140 caracteres.");
        if (nomeBeneficiario is { Length: > 200 })
            throw new InvalidOperationException("O nome do beneficiário do Pix deve ter no máximo 200 caracteres.");

        if (configuracao.Ativo)
        {
            if (string.IsNullOrWhiteSpace(chave))
                throw new InvalidOperationException("Informe a chave Pix.");
            if (string.IsNullOrWhiteSpace(nomeBeneficiario))
                throw new InvalidOperationException("Informe o nome do beneficiário do Pix.");
        }

        empresa.PixAtivo = configuracao.Ativo;
        empresa.PixChave = chave;
        empresa.PixNomeBeneficiario = nomeBeneficiario;
        await db.SaveChangesAsync();
    }

    public async Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, int clienteId)
    {
        if (empresaId <= 0 || pedidoId <= 0 || clienteId <= 0)
            return null;

        var pedido = await CarregarPedidoAsync(pedidoId);
        // Checagem de posse: mesma empresa E mesmo cliente. Não distingue "não existe" de
        // "existe mas não é seu" na mensagem/retorno — os dois casos voltam null, para não dar
        // pista de que outro pedido com aquele Id existe (evita enumeração de Id por terceiros).
        if (pedido is null || pedido.EmpresaId != empresaId || pedido.ClienteId != clienteId)
            return null;

        return MontarDto(pedido, podeConfirmar: false);
    }

    public async Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId)
    {
        var empresaId = empresaContext.RequireEmpresaId();
        if (pedidoId <= 0)
            return null;

        var pedido = await CarregarPedidoAsync(pedidoId);
        if (pedido is null || pedido.EmpresaId != empresaId)
            return null;

        return MontarDto(pedido, podeConfirmar: true);
    }

    public async Task<PagamentoPixDto> EnviarComprovanteAsync(
        int empresaId, int pedidoId, int clienteId, byte[] conteudo, string nomeArquivoOriginal, string contentTypeDeclarado)
    {
        var pedido = await CarregarPedidoParaEscritaAsync(pedidoId)
            ?? throw new InvalidOperationException("Pedido não encontrado.");

        if (pedido.EmpresaId != empresaId || pedido.ClienteId != clienteId)
            throw new InvalidOperationException("Pedido não encontrado.");
        if (pedido.FormaPagamento != FormaPagamento.Pix)
            throw new InvalidOperationException("Este pedido não é pago via Pix.");

        var statusAtual = pedido.StatusPagamento ?? StatusPagamento.Aguardando;
        if (statusAtual is not (StatusPagamento.Aguardando or StatusPagamento.Rejeitado))
            throw new InvalidOperationException(
                statusAtual == StatusPagamento.Pago
                    ? "Este pedido já teve o pagamento confirmado."
                    : "Este pedido já tem um comprovante em análise.");

        var caminhoAnterior = pedido.ComprovanteCaminho;
        var chave = await comprovantes.SalvarAsync(empresaId, pedidoId, conteudo, nomeArquivoOriginal, caminhoAnterior);

        pedido.ComprovanteCaminho = chave;
        pedido.ComprovanteNomeOriginal = nomeArquivoOriginal.Length > 255 ? nomeArquivoOriginal[..255] : nomeArquivoOriginal;
        pedido.ComprovanteContentType = ComprovanteArmazenamentoService.ContentTypePara(nomeArquivoOriginal);
        pedido.ComprovanteTamanhoBytes = conteudo.Length;
        pedido.ComprovanteEnviadoEm = DateTime.UtcNow;
        pedido.ComprovanteMotivoRejeicao = null;
        pedido.ComprovanteRevisadoPorUsuarioId = null;
        pedido.ComprovanteRevisadoEm = null;
        pedido.StatusPagamento = StatusPagamento.ComprovanteEnviado;

        await db.SaveChangesAsync();
        logger.LogInformation("Comprovante enviado para o pedido {PedidoId} pelo cliente {ClienteId}", pedidoId, clienteId);

        return MontarDto(pedido, podeConfirmar: false)!;
    }

    public async Task<ComprovanteArquivoDto?> ObterComprovanteParaClienteAsync(int empresaId, int pedidoId, int clienteId)
    {
        var pedido = await CarregarPedidoAsync(pedidoId);
        if (pedido is null || pedido.EmpresaId != empresaId || pedido.ClienteId != clienteId)
            return null;

        return await MontarArquivoAsync(pedido);
    }

    public async Task<ComprovanteArquivoDto?> ObterComprovanteParaAdministracaoAsync(int pedidoId)
    {
        var empresaId = empresaContext.RequireEmpresaId();
        var pedido = await CarregarPedidoAsync(pedidoId);
        if (pedido is null || pedido.EmpresaId != empresaId)
            return null;

        return await MontarArquivoAsync(pedido);
    }

    public async Task MarcarEmAnaliseAsync(int pedidoId, int usuarioId)
    {
        var empresaId = empresaContext.RequireEmpresaId();
        var pedido = await CarregarPedidoParaEscritaAsync(pedidoId)
            ?? throw new InvalidOperationException("Pedido não encontrado.");
        if (pedido.EmpresaId != empresaId)
            throw new InvalidOperationException("Pedido não encontrado.");

        // Só sai de ComprovanteEnviado — chamar de novo com o pedido já em EmAnalise/Pago/etc.
        // não é um erro do usuário (dois funcionários podem clicar quase ao mesmo tempo), então
        // é silenciosamente ignorado em vez de lançar.
        if (pedido.StatusPagamento != StatusPagamento.ComprovanteEnviado)
            return;

        pedido.StatusPagamento = StatusPagamento.EmAnalise;
        pedido.ComprovanteRevisadoPorUsuarioId = usuarioId;
        pedido.ComprovanteRevisadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ConfirmarRecebimentoAsync(int pedidoId, int usuarioId)
    {
        var empresaId = empresaContext.RequireEmpresaId();

        var pedido = await CarregarPedidoParaEscritaAsync(pedidoId)
            ?? throw new InvalidOperationException("Pedido não encontrado.");
        if (pedido.EmpresaId != empresaId)
            throw new InvalidOperationException("Pedido não encontrado.");
        if (pedido.FormaPagamento != FormaPagamento.Pix)
            throw new InvalidOperationException("Pedido não é Pix.");

        // Idempotente: confirmar um pedido já pago não reprocessa nem altera a data/usuário
        // originais — condição central de "duplicidade de confirmação" pedida no contrato.
        if (pedido.StatusPagamento == StatusPagamento.Pago)
            return;

        pedido.StatusPagamento = StatusPagamento.Pago;
        pedido.PagamentoConfirmadoEm = DateTime.UtcNow;
        pedido.PagamentoConfirmadoPorUsuarioId = usuarioId;
        await db.SaveChangesAsync();
        logger.LogInformation("Pagamento Pix do pedido {PedidoId} confirmado pelo usuário {UsuarioId}", pedidoId, usuarioId);
    }

    public async Task RejeitarComprovanteAsync(int pedidoId, int usuarioId, string motivo)
    {
        motivo = motivo?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(motivo))
            throw new InvalidOperationException("Informe o motivo da rejeição.");
        if (motivo.Length > 500)
            motivo = motivo[..500];

        var empresaId = empresaContext.RequireEmpresaId();
        var pedido = await CarregarPedidoParaEscritaAsync(pedidoId)
            ?? throw new InvalidOperationException("Pedido não encontrado.");
        if (pedido.EmpresaId != empresaId)
            throw new InvalidOperationException("Pedido não encontrado.");

        if (pedido.StatusPagamento is not (StatusPagamento.ComprovanteEnviado or StatusPagamento.EmAnalise))
            throw new InvalidOperationException(
                pedido.StatusPagamento == StatusPagamento.Pago
                    ? "Este pedido já teve o pagamento confirmado — não é possível rejeitar."
                    : "Este pedido não tem um comprovante pendente de revisão.");

        pedido.StatusPagamento = StatusPagamento.Rejeitado;
        pedido.ComprovanteMotivoRejeicao = motivo;
        pedido.ComprovanteRevisadoPorUsuarioId = usuarioId;
        pedido.ComprovanteRevisadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Comprovante do pedido {PedidoId} rejeitado pelo usuário {UsuarioId}: {Motivo}", pedidoId, usuarioId, motivo);
    }

    private async Task<ComprovanteArquivoDto?> MontarArquivoAsync(Pedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.ComprovanteCaminho))
            return null;

        var bytes = await comprovantes.LerAsync(pedido.ComprovanteCaminho);
        if (bytes is null)
        {
            logger.LogWarning("Comprovante do pedido {PedidoId} referenciado no banco mas ausente em disco: {Caminho}", pedido.Id, pedido.ComprovanteCaminho);
            return null;
        }

        return new ComprovanteArquivoDto
        {
            Conteudo = bytes,
            ContentType = pedido.ComprovanteContentType ?? "application/octet-stream",
            NomeArquivo = pedido.ComprovanteNomeOriginal ?? "comprovante"
        };
    }

    private PagamentoPixDto? MontarDto(Pedido pedido, bool podeConfirmar)
    {
        if (pedido.FormaPagamento != FormaPagamento.Pix)
            return null;

        var disponivel = pedido.Empresa.PixAtivo
            && !string.IsNullOrWhiteSpace(pedido.Empresa.PixChave)
            && !string.IsNullOrWhiteSpace(pedido.Empresa.PixNomeBeneficiario);
        // Pix antigo (anterior a esta funcionalidade) tem StatusPagamento nulo: interpretado como Aguardando.
        var status = pedido.StatusPagamento ?? StatusPagamento.Aguardando;
        var statusContrato = (StatusPagamentoPix)(int)status;
        var pago = status == StatusPagamento.Pago;

        string? payload = null;
        string? qrBase64 = null;
        if (disponivel && !pago)
        {
            try
            {
                payload = PixPayloadGerador.GerarPayload(new PixPayloadGerador.Dados(
                    ChavePix: pedido.Empresa.PixChave!,
                    NomeBeneficiario: pedido.Empresa.PixNomeBeneficiario!,
                    Cidade: null,
                    Valor: pedido.Total,
                    IdentificadorTransacao: $"PED{pedido.Id}"));
                qrBase64 = Convert.ToBase64String(QrCodeGerador.GerarPng(payload));
            }
            catch (Exception ex)
            {
                // Nunca deixa a geração do QR derrubar a consulta inteira — sem QR o cliente
                // ainda vê chave/valor/beneficiário e pode pagar manualmente digitando a chave.
                logger.LogError(ex, "Falha ao gerar payload/QR Pix para o pedido {PedidoId}", pedido.Id);
            }
        }

        return new PagamentoPixDto
        {
            PedidoId = pedido.Id,
            Valor = pedido.Total,
            Disponivel = disponivel,
            Chave = disponivel ? pedido.Empresa.PixChave : null,
            NomeBeneficiario = disponivel ? pedido.Empresa.PixNomeBeneficiario : null,
            Status = statusContrato,
            ConfirmadoEm = pedido.PagamentoConfirmadoEm,
            ConfirmadoPorNome = pedido.PagamentoConfirmadoPorUsuario?.Nome,
            // Confirmar não exige comprovante enviado: a regra de negócio é "a loja confere a
            // própria conta bancária" (comprovante é só um apoio, nunca confirma sozinho —
            // ver docs/CONTRATOS/PIX-MANUAL-001.md, regra preservada aqui). Por isso qualquer
            // status != Pago pode ser confirmado, não só ComprovanteEnviado/EmAnalise.
            PodeConfirmar = podeConfirmar && disponivel && status is not StatusPagamento.Pago,
            Simulado = false,
            MensagemIndisponibilidade = disponivel ? null : MensagemIndisponivel,
            PayloadCopiaECola = payload,
            QrCodePngBase64 = qrBase64,
            PodeEnviarComprovante = disponivel && status is StatusPagamento.Aguardando or StatusPagamento.Rejeitado,
            TemComprovante = !string.IsNullOrWhiteSpace(pedido.ComprovanteCaminho),
            ComprovanteNomeArquivo = pedido.ComprovanteNomeOriginal,
            ComprovanteEnviadoEm = pedido.ComprovanteEnviadoEm,
            ComprovanteMotivoRejeicao = status == StatusPagamento.Rejeitado ? pedido.ComprovanteMotivoRejeicao : null,
            ComprovanteRevisadoPorNome = pedido.ComprovanteRevisadoPorUsuario?.Nome,
            ComprovanteRevisadoEm = pedido.ComprovanteRevisadoEm
        };
    }

    private Task<Pedido?> CarregarPedidoAsync(int pedidoId) =>
        db.Pedidos.AsNoTracking()
            .Include(p => p.Empresa)
            .Include(p => p.PagamentoConfirmadoPorUsuario)
            .Include(p => p.ComprovanteRevisadoPorUsuario)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

    private Task<Pedido?> CarregarPedidoParaEscritaAsync(int pedidoId) =>
        db.Pedidos
            .Include(p => p.Empresa)
            .Include(p => p.PagamentoConfirmadoPorUsuario)
            .Include(p => p.ComprovanteRevisadoPorUsuario)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

    private async Task<Empresa> ObterEmpresaAutenticadaAsync()
    {
        var empresaId = empresaContext.RequireEmpresaId();
        return await db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId)
            ?? throw new InvalidOperationException("Empresa não encontrada.");
    }
}
