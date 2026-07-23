using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

/// <summary>
/// Venda de balcão (PDV) — distinta de <see cref="Pedido"/>, que representa encomenda com
/// prazo e fluxo de produção. Venda é imediata: cliente opcional, sem etapas de produção,
/// com forma de pagamento e cálculo de troco. Exige uma <see cref="SessaoCaixa"/> aberta.
/// Ver _ia/DECISOES.md (2026-07-01, "PDV como entidade nova" e "Caixa obrigatório").
/// </summary>
public class Venda
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public int SessaoCaixaId { get; set; }
    public SessaoCaixa SessaoCaixa { get; set; } = null!;
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public decimal ValorRecebido { get; set; }
    public decimal Troco { get; set; }
    public StatusVenda Status { get; set; } = StatusVenda.Finalizada;
    public ICollection<VendaItem> Itens { get; set; } = [];
}
