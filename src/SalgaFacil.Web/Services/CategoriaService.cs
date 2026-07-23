using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class CategoriaService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    private int EmpresaId => empresa.RequireEmpresaId();

    public Task<List<CategoriaProduto>> ListarAsync(bool? apenasAtivos = null) =>
        db.CategoriasProduto
            .Where(c => c.EmpresaId == EmpresaId)
            .Where(c => !apenasAtivos.HasValue || c.Ativo == apenasAtivos.Value)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .ToListAsync();

    public Task<CategoriaProduto?> ObterAsync(int id) =>
        db.CategoriasProduto.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId);

    public async Task SalvarAsync(CategoriaProduto categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nome))
            throw new InvalidOperationException("Nome da categoria é obrigatório.");

        categoria.Nome = categoria.Nome.Trim();
        var nomeExiste = await db.CategoriasProduto
            .AnyAsync(c => c.EmpresaId == EmpresaId && c.Nome == categoria.Nome && c.Id != categoria.Id);
        if (nomeExiste)
            throw new InvalidOperationException("Já existe uma categoria com este nome.");

        if (categoria.Id == 0)
        {
            var nova = new CategoriaProduto
            {
                EmpresaId = EmpresaId,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                Ordem = categoria.Ordem,
                Ativo = categoria.Ativo,
                DataCadastro = DateTime.UtcNow
            };
            db.CategoriasProduto.Add(nova);
            await db.SaveChangesAsync();
            categoria.Id = nova.Id;
            return;
        }

        var existente = await db.CategoriasProduto.FirstOrDefaultAsync(c => c.Id == categoria.Id && c.EmpresaId == EmpresaId)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        existente.Nome = categoria.Nome;
        existente.Descricao = categoria.Descricao;
        existente.Ordem = categoria.Ordem;
        existente.Ativo = categoria.Ativo;
        existente.DataAtualizacao = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task AlternarAtivoAsync(int id)
    {
        var cat = await db.CategoriasProduto.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId)
            ?? throw new InvalidOperationException("Categoria não encontrada.");
        cat.Ativo = !cat.Ativo;
        cat.DataAtualizacao = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var cat = await db.CategoriasProduto.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId);
        if (cat is null) return;

        var temProdutos = await db.Produtos.AnyAsync(p => p.CategoriaId == id && p.EmpresaId == EmpresaId);
        if (temProdutos)
            throw new InvalidOperationException("Não é possível excluir: existem produtos nesta categoria. Inative-a.");

        db.CategoriasProduto.Remove(cat);
        await db.SaveChangesAsync();
    }
}
