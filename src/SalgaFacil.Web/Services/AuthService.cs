using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

/// <summary>Contexto da empresa do usuário autenticado (área administrativa).</summary>
public interface IEmpresaContext
{
    int? EmpresaId { get; }
    int RequireEmpresaId();
}

public class EmpresaContext(AuthService auth) : IEmpresaContext
{
    public int? EmpresaId
    {
        get
        {
            var id = auth.UsuarioAtual?.EmpresaId;
            return id is > 0 ? id : null;
        }
    }

    public int RequireEmpresaId()
    {
        if (!auth.EstaAutenticado)
            throw new InvalidOperationException("Faça login para continuar.");

        return EmpresaId
            ?? throw new InvalidOperationException("Nenhuma empresa vinculada ao usuário autenticado. Saia e entre novamente.");
    }
}

/// <summary>
/// Auth em memória no circuito Blazor (scoped).
/// Sem ProtectedSessionStorage — ele travava o WebSocket neste projeto.
/// </summary>
public class AuthService(SalgaFacilDbContext db)
{
    public UsuarioSessao? UsuarioAtual { get; private set; }

    public Task RestaurarSessaoAsync() => Task.CompletedTask;

    public async Task<bool> LoginAsync(string email, string senha)
    {
        var hash = DbSeeder.HashSenha(senha);
        var usuario = await db.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Email == email && u.SenhaHash == hash && u.Ativo);

        if (usuario == null || usuario.EmpresaId <= 0 || usuario.Empresa is null || !usuario.Empresa.Ativo)
            return false;

        usuario.UltimoAcesso = DateTime.UtcNow;
        await db.SaveChangesAsync();

        UsuarioAtual = new UsuarioSessao
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            EmpresaId = usuario.EmpresaId,
            EmpresaNome = usuario.Empresa.NomeFantasia ?? usuario.Empresa.Nome,
            EmpresaSlug = usuario.Empresa.Slug,
            Papel = usuario.Papel
        };

        return true;
    }

    public Task LogoutAsync()
    {
        UsuarioAtual = null;
        return Task.CompletedTask;
    }

    public void Logout() => UsuarioAtual = null;

    public Task AtualizarSessaoEmpresaAsync(string nome, string slug)
    {
        if (UsuarioAtual is null) return Task.CompletedTask;
        UsuarioAtual.EmpresaNome = nome;
        UsuarioAtual.EmpresaSlug = slug;
        return Task.CompletedTask;
    }

    public bool EstaAutenticado => UsuarioAtual is { EmpresaId: > 0 };
}

public class UsuarioSessao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public string EmpresaSlug { get; set; } = string.Empty;
    public PapelUsuario Papel { get; set; }
    public string Iniciais => string.Concat(Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));
}
