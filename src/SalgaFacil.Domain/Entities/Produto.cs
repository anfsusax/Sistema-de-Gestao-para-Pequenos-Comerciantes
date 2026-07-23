using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

public class Produto
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public int CategoriaId { get; set; }
    public CategoriaProduto Categoria { get; set; } = null!;

    public int UnidadeMedidaId { get; set; }
    public UnidadeMedida UnidadeMedida { get; set; } = null!;

    /// <summary>Tipo de preparo (frito/assado) — independente da categoria comercial.</summary>
    public TipoProduto Tipo { get; set; }

    public string? FotoUrl { get; set; }
    public string? CodigoBarras { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal CustoEstimado { get; set; }
    public decimal EstoqueAtual { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }
}
