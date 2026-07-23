using SalgaFacil.Domain.Enums;

namespace SalgaFacil.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcesso { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public PapelUsuario Papel { get; set; } = PapelUsuario.Administrador;
}
