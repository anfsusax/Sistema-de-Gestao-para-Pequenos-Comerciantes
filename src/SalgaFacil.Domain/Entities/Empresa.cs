namespace SalgaFacil.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    /// <summary>Identificador público único da loja (ex.: salgados-da-consu).</summary>
    public string Slug { get; set; } = string.Empty;
    public string Nome { get; set; } = "SalgadosPro";
    public string? NomeFantasia { get; set; }
    public string? RazaoSocial { get; set; }
    public string? Cnpj { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Descricao { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? HorarioFuncionamento { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Usuario> Usuarios { get; set; } = [];
}
