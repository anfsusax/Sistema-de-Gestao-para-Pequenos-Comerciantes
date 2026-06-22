using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class AuthService(SalgaFacilDbContext db)
{
    public UsuarioSessao? UsuarioAtual { get; private set; }

    public async Task<bool> LoginAsync(string email, string senha)
    {
        var hash = DbSeeder.HashSenha(senha);
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.SenhaHash == hash && u.Ativo);
        if (usuario == null) return false;
        UsuarioAtual = new UsuarioSessao { Id = usuario.Id, Nome = usuario.Nome, Email = usuario.Email };
        return true;
    }

    public void Logout() => UsuarioAtual = null;

    public bool EstaAutenticado => UsuarioAtual != null;
}

public class UsuarioSessao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Iniciais => string.Concat(Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));
}
