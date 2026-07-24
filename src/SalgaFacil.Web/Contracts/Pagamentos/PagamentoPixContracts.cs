namespace SalgaFacil.Web.Contracts.Pagamentos;

public enum StatusPagamentoPix
{
    Aguardando = 1,
    Pago = 2
}

public sealed class ConfiguracaoPixDto
{
    public bool Ativo { get; set; }
    public string? Chave { get; set; }
    public string? NomeBeneficiario { get; set; }
    public bool Simulado { get; set; }
}

public sealed class PagamentoPixDto
{
    public int PedidoId { get; set; }
    public decimal Valor { get; set; }
    public bool Disponivel { get; set; }
    public string? Chave { get; set; }
    public string? NomeBeneficiario { get; set; }
    public StatusPagamentoPix Status { get; set; } = StatusPagamentoPix.Aguardando;
    public DateTime? ConfirmadoEm { get; set; }
    public bool PodeConfirmar { get; set; }
    public bool Simulado { get; set; }
    public string? MensagemIndisponibilidade { get; set; }
}

public interface IPagamentoPixService
{
    Task<ConfiguracaoPixDto> ObterConfiguracaoAsync();
    Task SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao);
    Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, string? telefone);
    Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId);
    Task ConfirmarRecebimentoAsync(int pedidoId);
}