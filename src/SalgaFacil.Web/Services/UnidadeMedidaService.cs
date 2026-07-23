using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class UnidadeMedidaService(SalgaFacilDbContext db)
{
    public Task<List<UnidadeMedida>> ListarAsync(bool? apenasAtivos = null) =>
        db.UnidadesMedida
            .Where(u => !apenasAtivos.HasValue || u.Ativo == apenasAtivos.Value)
            .OrderBy(u => u.Id)
            .ToListAsync();

    public Task<UnidadeMedida?> ObterAsync(int id) =>
        db.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == id);

    public async Task SalvarAsync(UnidadeMedida unidade)
    {
        if (string.IsNullOrWhiteSpace(unidade.Sigla) || string.IsNullOrWhiteSpace(unidade.Nome))
            throw new InvalidOperationException("Sigla e nome são obrigatórios.");

        unidade.Sigla = unidade.Sigla.Trim().ToUpperInvariant();
        unidade.Nome = unidade.Nome.Trim();

        var siglaExiste = await db.UnidadesMedida
            .AnyAsync(u => u.Sigla == unidade.Sigla && u.Id != unidade.Id);
        if (siglaExiste)
            throw new InvalidOperationException("Já existe uma unidade com esta sigla.");

        if (unidade.Id == 0)
        {
            var nova = new UnidadeMedida
            {
                Sigla = unidade.Sigla,
                Nome = unidade.Nome,
                Ativo = unidade.Ativo,
                DataCadastro = DateTime.UtcNow
            };
            db.UnidadesMedida.Add(nova);
            await db.SaveChangesAsync();
            unidade.Id = nova.Id;
            return;
        }

        var existente = await db.UnidadesMedida.FindAsync(unidade.Id)
            ?? throw new InvalidOperationException("Unidade não encontrada.");
        existente.Sigla = unidade.Sigla;
        existente.Nome = unidade.Nome;
        existente.Ativo = unidade.Ativo;
        existente.DataAtualizacao = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task AlternarAtivoAsync(int id)
    {
        var un = await db.UnidadesMedida.FindAsync(id)
            ?? throw new InvalidOperationException("Unidade não encontrada.");
        un.Ativo = !un.Ativo;
        un.DataAtualizacao = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var un = await db.UnidadesMedida.FindAsync(id);
        if (un is null) return;

        var temProdutos = await db.Produtos.AnyAsync(p => p.UnidadeMedidaId == id);
        if (temProdutos)
            throw new InvalidOperationException("Não é possível excluir: existem produtos com esta unidade. Inative-a.");

        db.UnidadesMedida.Remove(un);
        await db.SaveChangesAsync();
    }
}
