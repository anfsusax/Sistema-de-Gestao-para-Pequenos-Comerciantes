namespace SalgaFacil.Domain.Entities;

public class UnidadeMedida
{
    public int Id { get; set; }
    public string Sigla { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }

    public ICollection<Produto> Produtos { get; set; } = [];
}
