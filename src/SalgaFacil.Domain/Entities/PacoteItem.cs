namespace SalgaFacil.Domain.Entities;

public class PacoteItem
{
    public int Id { get; set; }
    public int PacoteId { get; set; }
    public Pacote Pacote { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int Quantidade { get; set; }
}
