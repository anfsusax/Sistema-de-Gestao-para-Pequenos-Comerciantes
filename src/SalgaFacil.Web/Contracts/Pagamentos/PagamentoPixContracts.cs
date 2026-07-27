namespace SalgaFacil.Web.Contracts.Pagamentos;

/// <summary>
/// Contrato Pix manual — v2 (PIX-MANUAL-002, 2026-07-25). Supera a v1 congelada em
/// docs/CONTRATOS/PIX-MANUAL-001.md (já integrada e concluída): adiciona QR Code/copia-e-cola,
/// upload e revisão de comprovante, e os 3 novos estados de status. Documentado em
/// docs/RELATORIOS/PIX-MANUAL-002-FLUXO-COMPLETO.md.
/// </summary>
public enum StatusPagamentoPix
{
    Aguardando = 1,
    Pago = 2,
    ComprovanteEnviado = 3,
    EmAnalise = 4,
    Rejeitado = 5
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
    public string? ConfirmadoPorNome { get; set; }
    public bool PodeConfirmar { get; set; }
    public bool Simulado { get; set; }
    public string? MensagemIndisponibilidade { get; set; }

    // PIX-MANUAL-002 — pagamento em si (QR/copia-e-cola)
    /// <summary>Texto "Pix Copia e Cola" (BR Code/EMV completo, com CRC16). Nulo quando indisponível.</summary>
    public string? PayloadCopiaECola { get; set; }
    /// <summary>PNG do QR Code do payload acima, em base64 puro (sem prefixo "data:"). Nulo quando indisponível.</summary>
    public string? QrCodePngBase64 { get; set; }

    // PIX-MANUAL-002 — comprovante
    /// <summary>True quando o cliente pode (re)enviar um comprovante agora (status Aguardando ou Rejeitado).</summary>
    public bool PodeEnviarComprovante { get; set; }
    public bool TemComprovante { get; set; }
    public string? ComprovanteNomeArquivo { get; set; }
    public DateTime? ComprovanteEnviadoEm { get; set; }
    public string? ComprovanteMotivoRejeicao { get; set; }
    public string? ComprovanteRevisadoPorNome { get; set; }
    public DateTime? ComprovanteRevisadoEm { get; set; }
}

/// <summary>Bytes de um comprovante já autorizado — só devolvido por métodos que já checaram propriedade do pedido.</summary>
public sealed class ComprovanteArquivoDto
{
    public byte[] Conteudo { get; set; } = [];
    public string ContentType { get; set; } = "";
    public string NomeArquivo { get; set; } = "";
}

public interface IPagamentoPixService
{
    Task<ConfiguracaoPixDto> ObterConfiguracaoAsync();
    Task SalvarConfiguracaoAsync(ConfiguracaoPixDto configuracao);

    /// <summary>
    /// Consulta para o cliente DONO do pedido — autorização por ClienteId autenticado (não por
    /// telefone; ver docs/RELATORIOS/PIX-MANUAL-002-FLUXO-COMPLETO.md para a justificativa da
    /// mudança em relação à v1). Retorna null se o pedido não existir, não for desta empresa, ou
    /// não pertencer a este cliente — sem distinguir a causa, para não revelar existência de
    /// pedidos de terceiros.
    /// </summary>
    Task<PagamentoPixDto?> ObterParaClienteAsync(int empresaId, int pedidoId, int clienteId);

    /// <summary>Consulta para a área administrativa — autorização por empresa do usuário autenticado (via IEmpresaContext).</summary>
    Task<PagamentoPixDto?> ObterParaAdministracaoAsync(int pedidoId);

    /// <summary>
    /// Cliente envia (ou reenvia, após rejeição) o comprovante. Válido apenas quando o status
    /// atual é Aguardando ou Rejeitado — lança <see cref="InvalidOperationException"/> fora
    /// disso (inclusive se já Pago, protegendo contra reenvio depois da confirmação).
    /// </summary>
    Task<PagamentoPixDto> EnviarComprovanteAsync(
        int empresaId, int pedidoId, int clienteId, byte[] conteudo, string nomeArquivoOriginal, string contentTypeDeclarado);

    /// <summary>Bytes do comprovante para o cliente dono do pedido. Null se não houver comprovante ou não autorizado.</summary>
    Task<ComprovanteArquivoDto?> ObterComprovanteParaClienteAsync(int empresaId, int pedidoId, int clienteId);

    /// <summary>Bytes do comprovante para a administração da empresa dona do pedido. Null se não houver comprovante ou não autorizado.</summary>
    Task<ComprovanteArquivoDto?> ObterComprovanteParaAdministracaoAsync(int pedidoId);

    /// <summary>Marca "em análise" (transição informativa, opcional) — só sai de ComprovanteEnviado.</summary>
    Task MarcarEmAnaliseAsync(int pedidoId, int usuarioId);

    /// <summary>
    /// Confirma o recebimento do Pix. Idempotente: reconfirmar um pedido já Pago não reprocessa
    /// nem sobrescreve a data/usuário da primeira confirmação.
    /// </summary>
    Task ConfirmarRecebimentoAsync(int pedidoId, int usuarioId);

    /// <summary>Rejeita o comprovante atual, com motivo obrigatório — cliente pode reenviar depois.</summary>
    Task RejeitarComprovanteAsync(int pedidoId, int usuarioId, string motivo);
}
