using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalgaFacil.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SalgaFacil.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalgaFacilDbContext>();

        // Prefere migrations. Se o histórico não existir (DB antigo com EnsureCreated),
        // em Development recria o schema via Migrate.
        try
        {
            await db.Database.MigrateAsync();
        }
        catch
        {
            var isDev = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);
            if (!isDev)
                throw;

            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        if (await db.Usuarios.AnyAsync()) return;

        // Seed mínimo: login + empresa + unidades padrão.
        // Categorias, produtos, clientes e pedidos ficam vazios para cadastro do zero.
        db.Usuarios.Add(new Usuario
        {
            Nome = "Maria Barbosa",
            Email = "maria@salgadospro.com",
            SenhaHash = HashSenha("123456"),
            Ativo = true
        });

        db.Empresas.Add(new Empresa
        {
            Nome = "SalgadosPro",
            Telefone = "(11) 99999-0000",
            Email = "contato@salgadospro.com"
        });

        db.UnidadesMedida.AddRange(
            new UnidadeMedida { Sigla = "UN", Nome = "Unidade", Ativo = true },
            new UnidadeMedida { Sigla = "KG", Nome = "Quilograma", Ativo = true },
            new UnidadeMedida { Sigla = "G", Nome = "Grama", Ativo = true },
            new UnidadeMedida { Sigla = "L", Nome = "Litro", Ativo = true },
            new UnidadeMedida { Sigla = "ML", Nome = "Mililitro", Ativo = true },
            new UnidadeMedida { Sigla = "CX", Nome = "Caixa", Ativo = true },
            new UnidadeMedida { Sigla = "PCT", Nome = "Pacote", Ativo = true });

        await db.SaveChangesAsync();
    }

    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}
