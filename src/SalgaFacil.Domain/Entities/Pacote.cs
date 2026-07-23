namespace SalgaFacil.Domain.Entities;

public class Pacote
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;
    public int QuantidadeTotal { get; set; }
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
    public ICollection<PacoteItem> Itens { get; set; } = [];
}
