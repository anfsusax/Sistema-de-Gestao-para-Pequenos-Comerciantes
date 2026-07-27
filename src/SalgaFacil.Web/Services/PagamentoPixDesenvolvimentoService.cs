using SalgaFacil.Web.Contracts.Pagamentos;

namespace SalgaFacil.Web.Services;

/// <summary>
/// Fake determinístico para desenvolvimento paralelo do Frontend (histórico de PIX-MANUAL-001 —
/// ver docs/CONTRATOS/PIX-MANUAL-001.md, seção "Desenvolvimento Paralelo"). Não está registrado
/// em Program.cs hoje (o Principal usa <see cref="PagamentoPixService"/> real desde a
/// integração); mantido só para não quebrar quem ainda referenciar este tipo em um worktree
/// isolado. Nunca persiste ou processa pagamento real; comprovante não é realmente armazenado
/// (fica só em memória, junto com o resto do estado simulado deste fake).
/// </summary>
public sealed class PagamentoPixDesenvolvimentoService(IHostEnvironment environment) : IPagamentoPixService
{
    private ConfiguracaoPixDto _configuracao = new()
    {
        Ativo = true,
        Chave = "CHAVE-PIX-SIMULADA",
        NomeBeneficiario = "Loja de desenvolvimento",
        Simulado = true
    };
    private readonly Dictionary<int, StatusPagamentoPix> _statusPorPedido = [];
    private readonly Dictionary<int, string> _comprovantesPorPedido = [];

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

    public Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, int clienteId)
    {
        ExigirDevelopment();
        if (empresaId <= 0 || pedidoId <= 0 || clienteId <= 0)
            return Task.FromResult<PagamentoPixDto?>(null);
        return Task.FromResult<PagamentoPixDto?>(CriarPagamento(pedidoId));
    }

    public Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId)
    {
        ExigirDevelopment();
        return Task.FromResult<PagamentoPixDto?>(pedidoId <= 0 ? null : CriarPagamento(pedidoId));
    }

    public Task<PagamentoPixDto> EnviarComprovanteAsync(
        int empresaId, int pedidoId, int clienteId, byte[] conteudo, string nomeArquivoOriginal, string contentTypeDeclarado)
    {
        ExigirDevelopment();
        if (pedidoId <= 0) throw new InvalidOperationException("Pedido inválido.");
        _comprovantesPorPedido[pedidoId] = nomeArquivoOriginal;
        _statusPorPedido[pedidoId] = StatusPagamentoPix.ComprovanteEnviado;
        return Task.FromResult(CriarPagamento(pedidoId));
    }

    public Task<ComprovanteArquivoDto?> ObterComprovanteParaClienteAsync(int empresaId, int pedidoId, int clienteId)
    {
        ExigirDevelopment();
        return Task.FromResult(CriarComprovanteFake(pedidoId));
    }

    public Task<ComprovanteArquivoDto?> ObterComprovanteParaAdministracaoAsync(int pedidoId)
    {
        ExigirDevelopment();
        return Task.FromResult(CriarComprovanteFake(pedidoId));
    }

    public Task MarcarEmAnaliseAsync(int pedidoId, int usuarioId)
    {
        ExigirDevelopment();
        if (_statusPorPedido.GetValueOrDefault(pedidoId) == StatusPagamentoPix.ComprovanteEnviado)
            _statusPorPedido[pedidoId] = StatusPagamentoPix.EmAnalise;
        return Task.CompletedTask;
    }

    public Task ConfirmarRecebimentoAsync(int pedidoId, int usuarioId)
    {
        ExigirDevelopment();
        if (pedidoId <= 0) throw new InvalidOperationException("Pedido inválido.");
        _statusPorPedido[pedidoId] = StatusPagamentoPix.Pago;
        return Task.CompletedTask;
    }

    public Task RejeitarComprovanteAsync(int pedidoId, int usuarioId, string motivo)
    {
        ExigirDevelopment();
        if (string.IsNullOrWhiteSpace(motivo)) throw new InvalidOperationException("Informe o motivo da rejeição.");
        _statusPorPedido[pedidoId] = StatusPagamentoPix.Rejeitado;
        return Task.CompletedTask;
    }

    private ComprovanteArquivoDto? CriarComprovanteFake(int pedidoId) =>
        _comprovantesPorPedido.TryGetValue(pedidoId, out var nome)
            ? new ComprovanteArquivoDto { Conteudo = [0x25, 0x50, 0x44, 0x46], ContentType = "application/pdf", NomeArquivo = nome }
            : null;

    private PagamentoPixDto CriarPagamento(int pedidoId)
    {
        var disponivel = _configuracao.Ativo && !string.IsNullOrWhiteSpace(_configuracao.Chave);
        var status = _statusPorPedido.GetValueOrDefault(pedidoId, StatusPagamentoPix.Aguardando);
        var pago = status == StatusPagamentoPix.Pago;

        return new PagamentoPixDto
        {
            PedidoId = pedidoId,
            Valor = 42.50m,
            Disponivel = disponivel,
            Chave = disponivel ? _configuracao.Chave : null,
            NomeBeneficiario = disponivel ? _configuracao.NomeBeneficiario : null,
            Status = status,
            ConfirmadoEm = pago ? DateTime.UtcNow : null,
            ConfirmadoPorNome = pago ? "Simulado" : null,
            PodeConfirmar = disponivel && !pago,
            Simulado = true,
            MensagemIndisponibilidade = disponivel ? null : "Pagamento Pix indisponível no momento. Entre em contato com a loja.",
            PayloadCopiaECola = disponivel && !pago ? "00020126SIMULADO_NAO_USAR_EM_PRODUCAO5204000053039865802BR6304FAKE" : null,
            QrCodePngBase64 = null,
            PodeEnviarComprovante = disponivel && status is StatusPagamentoPix.Aguardando or StatusPagamentoPix.Rejeitado,
            TemComprovante = _comprovantesPorPedido.ContainsKey(pedidoId),
            ComprovanteNomeArquivo = _comprovantesPorPedido.GetValueOrDefault(pedidoId),
            ComprovanteMotivoRejeicao = status == StatusPagamentoPix.Rejeitado ? "Motivo simulado (Development)" : null
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

/// <summary>Implementação segura para quando o Pix real não pode ser registrado (histórico de PIX-MANUAL-001) — nunca usada em Production hoje, mantida por compatibilidade.</summary>
public sealed class PagamentoPixIndisponivelService : IPagamentoPixService
{
    private const string Mensagem = "Pagamento Pix indisponível no momento. Entre em contato com a loja.";

    public Task<ConfiguracaoPixDto> ObterConfiguracaoAsync() => Task.FromResult(new ConfiguracaoPixDto());
    public Task SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao) => Task.FromException(new InvalidOperationException(Mensagem));
    public Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, int clienteId) => Task.FromResult<PagamentoPixDto?>(null);
    public Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId) => Task.FromResult<PagamentoPixDto?>(null);
    public Task<PagamentoPixDto> EnviarComprovanteAsync(int empresaId, int pedidoId, int clienteId, byte[] conteudo, string nomeArquivoOriginal, string contentTypeDeclarado) =>
        Task.FromException<PagamentoPixDto>(new InvalidOperationException(Mensagem));
    public Task<ComprovanteArquivoDto?> ObterComprovanteParaClienteAsync(int empresaId, int pedidoId, int clienteId) => Task.FromResult<ComprovanteArquivoDto?>(null);
    public Task<ComprovanteArquivoDto?> ObterComprovanteParaAdministracaoAsync(int pedidoId) => Task.FromResult<ComprovanteArquivoDto?>(null);
    public Task MarcarEmAnaliseAsync(int pedidoId, int usuarioId) => Task.FromException(new InvalidOperationException(Mensagem));
    public Task ConfirmarRecebimentoAsync(int pedidoId, int usuarioId) => Task.FromException(new InvalidOperationException(Mensagem));
    public Task RejeitarComprovanteAsync(int pedidoId, int usuarioId, string motivo) => Task.FromException(new InvalidOperationException(Mensagem));
}
