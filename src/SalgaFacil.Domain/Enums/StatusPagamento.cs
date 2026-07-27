namespace SalgaFacil.Domain.Enums;

/// <summary>
/// Status de pagamento Pix persistido em <see cref="Entities.Pedido.StatusPagamento"/>.
/// Espelha os valores de <c>StatusPagamentoPix</c> (contrato em
/// SalgaFacil.Web.Contracts.Pagamentos.PagamentoPixContracts) para permitir conversão direta
/// por valor numérico, sem acoplar o Domain à camada Web.
///
/// PIX-MANUAL-002 (2026-07-25) — estendido de 2 para 5 estados para suportar o fluxo completo
/// de comprovante: cliente envia um arquivo, a loja analisa e confirma ou rejeita.
/// Transições válidas:
///
///   Aguardando ──(cliente envia comprovante)──► ComprovanteEnviado
///   ComprovanteEnviado ──(loja começa a revisar, opcional)──► EmAnalise
///   ComprovanteEnviado ou EmAnalise ──(loja confirma)──► Pago [estado final]
///   ComprovanteEnviado ou EmAnalise ──(loja rejeita, com motivo)──► Rejeitado
///   Rejeitado ──(cliente reenvia)──► ComprovanteEnviado
///
/// Pago é terminal: nenhuma transição sai dele (confirmação é idempotente — reconfirmar não
/// reprocessa). Aguardando/Rejeitado são os únicos estados em que um novo envio de comprovante
/// é aceito (ver PagamentoPixService.EnviarComprovanteAsync).
/// </summary>
public enum StatusPagamento
{
    Aguardando = 1,
    Pago = 2,
    ComprovanteEnviado = 3,
    EmAnalise = 4,
    Rejeitado = 5
}
