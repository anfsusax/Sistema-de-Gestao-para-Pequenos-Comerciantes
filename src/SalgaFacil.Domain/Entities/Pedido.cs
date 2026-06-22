using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public DateTime? DataEntregaPrevista { get; set; }
    public StatusPedido Status { get; set; } = StatusPedido.Aguardando;
    public decimal Total { get; set; }
    public ICollection<PedidoItem> Itens { get; set; } = [];
}
