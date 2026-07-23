using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class EmpresaService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    public Task<Empresa?> ObterAsync() =>
        db.Empresas.FirstOrDefaultAsync(e => e.Id == empresa.RequireEmpresaId());

    public Task<Empresa?> ObterPorSlugAsync(string slug) =>
        db.Empresas.FirstOrDefaultAsync(e => e.Slug == slug && e.Ativo);

    public async Task SalvarAsync(Empresa dados)
    {
        var empresaId = empresa.RequireEmpresaId();
        var existente = await db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId)
            ?? throw new InvalidOperationException("Empresa não encontrada.");

        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new InvalidOperationException("Nome da empresa é obrigatório.");
        if (string.IsNullOrWhiteSpace(dados.Slug))
            throw new InvalidOperationException("Slug da loja é obrigatório.");

        var slug = dados.Slug.Trim().ToLowerInvariant();
        var slugEmUso = await db.Empresas.AnyAsync(e => e.Slug == slug && e.Id != empresaId);
        if (slugEmUso)
            throw new InvalidOperationException("Este slug já está em uso por outra loja.");

        existente.Nome = dados.Nome.Trim();
        existente.NomeFantasia = dados.NomeFantasia;
        existente.RazaoSocial = dados.RazaoSocial;
        existente.Cnpj = dados.Cnpj;
        existente.Telefone = dados.Telefone;
        existente.WhatsApp = dados.WhatsApp;
        existente.Email = dados.Email;
        existente.Endereco = dados.Endereco;
        existente.Descricao = dados.Descricao;
        existente.LogoUrl = dados.LogoUrl;
        existente.BannerUrl = dados.BannerUrl;
        existente.HorarioFuncionamento = dados.HorarioFuncionamento;
        existente.Instagram = dados.Instagram;
        existente.Facebook = dados.Facebook;
        existente.Slug = slug;
        existente.Ativo = dados.Ativo;

        await db.SaveChangesAsync();
    }
}
