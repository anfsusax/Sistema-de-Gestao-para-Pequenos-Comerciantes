namespace SalgaFacil.Domain.Entities;

public class PedidoItem
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public int? PacoteId { get; set; }
    public Pacote? Pacote { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Total { get; set; }
}
