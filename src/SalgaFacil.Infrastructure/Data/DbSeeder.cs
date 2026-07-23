using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace SalgaFacil.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalgaFacilDbContext>();

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

        await GarantirEmpresaEUsuarioAsync(db);
    }

    private static async Task GarantirEmpresaEUsuarioAsync(SalgaFacilDbContext db)
    {
        if (!await db.UnidadesMedida.AnyAsync())
        {
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

        var empresa = await db.Empresas.OrderBy(e => e.Id).FirstOrDefaultAsync();
        if (empresa is null)
        {
            empresa = new Empresa
            {
                Slug = "consucruz",
                Nome = "ConsuCruz LTDA",
                NomeFantasia = "ConsuCruz",
                RazaoSocial = "ConsuCruz LTDA",
                Telefone = "(11) 99999-0000",
                WhatsApp = "(11) 99999-0000",
                Email = "contato@consucruz.com",
                Endereco = "Suzano — SP",
                Descricao = "Salgados frescos, kits e encomendas para o seu dia a dia.",
                HorarioFuncionamento = "Seg–Sáb · 8h às 20h",
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            db.Empresas.Add(empresa);
            await db.SaveChangesAsync();
        }
        else
        {
            // Garante nome/slug ConsuCruz na empresa principal (dev/MVP).
            empresa.Nome = "ConsuCruz LTDA";
            empresa.NomeFantasia = "ConsuCruz";
            empresa.RazaoSocial = "ConsuCruz LTDA";
            if (string.IsNullOrWhiteSpace(empresa.Slug) || empresa.Slug is "salgados-da-consu" or "salgadospro")
                empresa.Slug = "consucruz";
            empresa.Ativo = true;
            await db.SaveChangesAsync();
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "maria@salgadospro.com");
        if (usuario is null)
        {
            db.Usuarios.Add(new Usuario
            {
                Nome = "Maria Barbosa",
                Email = "maria@salgadospro.com",
                SenhaHash = HashSenha("123456"),
                Ativo = true,
                EmpresaId = empresa.Id,
                Papel = PapelUsuario.Administrador,
                DataCadastro = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else if (usuario.EmpresaId != empresa.Id)
        {
            usuario.EmpresaId = empresa.Id;
            usuario.Ativo = true;
            await db.SaveChangesAsync();
        }
    }

    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToBase64String(bytes);
    }
}
