using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Domain.Services;
using SalgaFacil.Infrastructure.Data;
using SalgaFacil.Web.Contracts.Pagamentos;

namespace SalgaFacil.Web.Services;

/// <summary>
/// Implementação real de <see cref="IPagamentoPixService"/> (PIX-MANUAL-001). Sem gateway,
/// webhook, cartão ou credencial bancária — o comerciante autenticado confirma manualmente o
/// recebimento visto no aplicativo bancário dele. O cliente só consulta; nunca confirma o
/// próprio pagamento (contrato: docs/CONTRATOS/PIX-MANUAL-001.md).
/// </summary>
public sealed class PagamentoPixService(SalgaFacilDbContext db, IEmpresaContext empresaContext) : IPagamentoPixService
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

    public async Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, string? telefone)
    {
        if (empresaId <= 0 || pedidoId <= 0)
            return null;

        var telefoneNormalizado = TelefoneNormalizador.Normalizar(telefone);
        if (telefoneNormalizado is null)
            return null;

        var pedido = await db.Pedidos
            .Include(p => p.Empresa)
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == pedidoId && p.EmpresaId == empresaId);
        if (pedido is null)
            return null;

        var telefoneCliente = pedido.Cliente.TelefoneNormalizado
            ?? TelefoneNormalizador.Normalizar(pedido.Cliente.Telefone)
            ?? TelefoneNormalizador.Normalizar(pedido.Cliente.WhatsApp);
        if (telefoneCliente is null || telefoneCliente != telefoneNormalizado)
            return null;

        return MontarDto(pedido, podeConfirmar: false);
    }

    public async Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId)
    {
        var empresaId = empresaContext.RequireEmpresaId();
        if (pedidoId <= 0)
            return null;

        var pedido = await db.Pedidos
            .Include(p => p.Empresa)
            .FirstOrDefaultAsync(p => p.Id == pedidoId && p.EmpresaId == empresaId);
        if (pedido is null)
            return null;

        return MontarDto(pedido, podeConfirmar: true);
    }

    public async Task ConfirmarRecebimentoAsync(int pedidoId)
    {
        var empresaId = empresaContext.RequireEmpresaId();

        var pedido = await db.Pedidos
            .FirstOrDefaultAsync(p => p.Id == pedidoId && p.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Pedido não encontrado.");

        if (pedido.FormaPagamento != FormaPagamento.Pix)
            throw new InvalidOperationException("Pedido não é Pix.");

        // Idempotente: confirmar um pedido já pago não reprocessa nem altera a data original.
        if (pedido.StatusPagamento == StatusPagamento.Pago)
            return;

        pedido.StatusPagamento = StatusPagamento.Pago;
        pedido.PagamentoConfirmadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static PagamentoPixDto? MontarDto(Pedido pedido, bool podeConfirmar)
    {
        if (pedido.FormaPagamento != FormaPagamento.Pix)
            return null;

        var disponivel = pedido.Empresa.PixAtivo
            && !string.IsNullOrWhiteSpace(pedido.Empresa.PixChave)
            && !string.IsNullOrWhiteSpace(pedido.Empresa.PixNomeBeneficiario);
        // Pix antigo (anterior a esta funcionalidade) tem StatusPagamento nulo: interpretado como Aguardando.
        var status = pedido.StatusPagamento ?? StatusPagamento.Aguardando;
        var pago = status == StatusPagamento.Pago;

        return new PagamentoPixDto
        {
            PedidoId = pedido.Id,
            Valor = pedido.Total,
            Disponivel = disponivel,
            Chave = disponivel ? pedido.Empresa.PixChave : null,
            NomeBeneficiario = disponivel ? pedido.Empresa.PixNomeBeneficiario : null,
            Status = pago ? StatusPagamentoPix.Pago : StatusPagamentoPix.Aguardando,
            ConfirmadoEm = pedido.PagamentoConfirmadoEm,
            PodeConfirmar = podeConfirmar && disponivel && !pago,
            Simulado = false,
            MensagemIndisponibilidade = disponivel ? null : MensagemIndisponivel
        };
    }

    private async Task<Empresa> ObterEmpresaAutenticadaAsync()
    {
        var empresaId = empresaContext.RequireEmpresaId();
        return await db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId)
            ?? throw new InvalidOperationException("Empresa não encontrada.");
    }
}
