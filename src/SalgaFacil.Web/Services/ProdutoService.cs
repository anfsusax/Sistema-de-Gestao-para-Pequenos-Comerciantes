using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class ProdutoService(SalgaFacilDbContext db)
{
    public Task<List<Produto>> ListarAsync(string? filtroTipo = null, bool? apenasAtivos = null) =>
        db.Produtos
            .Where(p => filtroTipo == null || (filtroTipo == "Frito" && p.Tipo == TipoProduto.Frito) || (filtroTipo == "Assado" && p.Tipo == TipoProduto.Assado))
            .Where(p => !apenasAtivos.HasValue || p.Ativo == apenasAtivos.Value)
            .OrderBy(p => p.Nome)
            .ToListAsync();

    public Task<Produto?> ObterAsync(int id) => db.Produtos.FindAsync(id).AsTask();

    public async Task SalvarAsync(Produto produto)
    {
        if (produto.Id == 0)
            db.Produtos.Add(produto);
        else
            db.Produtos.Update(produto);
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var produto = await db.Produtos.FindAsync(id);
        if (produto != null)
        {
            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();
        }
    }
}
