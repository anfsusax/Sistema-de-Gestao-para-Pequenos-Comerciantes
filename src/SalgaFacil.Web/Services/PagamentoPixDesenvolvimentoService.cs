using SalgaFacil.Web.Contracts.Pagamentos;

namespace SalgaFacil.Web.Services;

/// <summary>Fake determinístico para desenvolvimento paralelo do Frontend. Nunca persiste ou processa pagamento real.</summary>
public sealed class PagamentoPixDesenvolvimentoService(IHostEnvironment environment) : IPagamentoPixService
{
    private ConfiguracaoPixDto _configuracao = new()
    {
        Ativo = true,
        Chave = "CHAVE-PIX-SIMULADA",
        NomeBeneficiario = "Loja de desenvolvimento",
        Simulado = true
    };
    private readonly HashSet<int> _pedidosPagos = [];

    public Task<ConfiguracaoPixDto> ObterConfiguracaoAsync()
    {
        ExigirDevelopment();
        return Task.FromResult(Copiar(_configuracao));
    }

    public Task SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao)
    {
        ExigirDevelopment();
        Validar(configuracao);
        _configuracao = new ConfiguracaoPixDto
        {
            Ativo = configuracao.Ativo,
            Chave = configuracao.Chave?.Trim(),
            NomeBeneficiario = configuracao.NomeBeneficiario?.Trim(),
            Simulado = true
        };
        return Task.CompletedTask;
    }

    public Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, string? telefone)
    {
        ExigirDevelopment();
        if (empresaId <= 0 || pedidoId <= 0 || string.IsNullOrWhiteSpace(telefone))
            return Task.FromResult<PagamentoPixDto?>(null);
        return Task.FromResult<PagamentoPixDto?>(CriarPagamento(pedidoId));
    }

    public Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId)
    {
        ExigirDevelopment();
        return Task.FromResult<PagamentoPixDto?>(pedidoId <= 0 ? null : CriarPagamento(pedidoId));
    }

    public Task ConfirmarRecebimentoAsync(int pedidoId)
    {
        ExigirDevelopment();
        if (pedidoId <= 0) throw new InvalidOperationException("Pedido inválido.");
        _pedidosPagos.Add(pedidoId);
        return Task.CompletedTask;
    }

    private PagamentoPixDto CriarPagamento(int pedidoId)
    {
        var disponivel = _configuracao.Ativo && !string.IsNullOrWhiteSpace(_configuracao.Chave);
        var pago = _pedidosPagos.Contains(pedidoId);
        return new PagamentoPixDto
        {
            PedidoId = pedidoId,
            Valor = 42.50m,
            Disponivel = disponivel,
            Chave = disponivel ? _configuracao.Chave : null,
            NomeBeneficiario = disponivel ? _configuracao.NomeBeneficiario : null,
            Status = pago ? StatusPagamentoPix.Pago : StatusPagamentoPix.Aguardando,
            ConfirmadoEm = pago ? DateTime.UtcNow : null,
            PodeConfirmar = disponivel && !pago,
            Simulado = true,
            MensagemIndisponibilidade = disponivel ? null : "Pagamento Pix indisponível no momento. Entre em contato com a loja."
        };
    }

    private static ConfiguracaoPixDto Copiar(ConfiguracaoPixDto origem) => new()
    {
        Ativo = origem.Ativo,
        Chave = origem.Chave,
        NomeBeneficiario = origem.NomeBeneficiario,
        Simulado = true
    };

    private static void Validar(ConfiguracaoPixDto configuracao)
    {
        if (!configuracao.Ativo) return;
        if (string.IsNullOrWhiteSpace(configuracao.Chave)) throw new InvalidOperationException("Informe a chave Pix.");
        if (string.IsNullOrWhiteSpace(configuracao.NomeBeneficiario)) throw new InvalidOperationException("Informe o nome do beneficiário do Pix.");
    }

    private void ExigirDevelopment()
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("O serviço Pix simulado só pode ser usado em Development.");
    }
}

public sealed class PagamentoPixIndisponivelService : IPagamentoPixService
{
    private const string Mensagem = "Pagamento Pix indisponível no momento. Entre em contato com a loja.";

    public Task<ConfiguracaoPixDto> ObterConfiguracaoAsync() => Task.FromResult(new ConfiguracaoPixDto());
    public Task SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao) => Task.FromException(new InvalidOperationException(Mensagem));
    public Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, string? telefone) => Task.FromResult<PagamentoPixDto?>(null);
    public Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId) => Task.FromResult<PagamentoPixDto?>(null);
    public Task ConfirmarRecebimentoAsync(int pedidoId) => Task.FromException(new InvalidOperationException(Mensagem));
}