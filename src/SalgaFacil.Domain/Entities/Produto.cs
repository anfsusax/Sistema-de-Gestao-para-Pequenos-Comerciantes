using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Salgado";
    public TipoProduto Tipo { get; set; }
    public string? Descricao { get; set; }
    public string? FotoUrl { get; set; }
    public string? CodigoBarras { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal CustoEstimado { get; set; }
    public int EstoqueAtual { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
