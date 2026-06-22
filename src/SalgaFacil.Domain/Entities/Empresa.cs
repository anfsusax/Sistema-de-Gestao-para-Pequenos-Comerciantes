namespace SalgaFacil.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Nome { get; set; } = "SalgadosPro";
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Email { get; set; }
}
