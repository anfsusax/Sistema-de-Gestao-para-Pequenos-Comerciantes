namespace SalgaFacil.Domain.Enums;

/// <summary>
/// Status de pagamento Pix persistido em <see cref="Entities.Pedido.StatusPagamento"/>.
/// Espelha os valores de <c>StatusPagamentoPix</c> (contrato congelado em
/// SalgaFacil.Web.Contracts.Pagamentos.PagamentoPixContracts) para permitir conversão direta
/// por valor numérico, sem acoplar o Domain à camada Web.
/// </summary>
public enum StatusPagamento
{
    Aguardando = 1,
    Pago = 2
}
