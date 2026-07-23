using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class PacoteService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    private int EmpresaId => empresa.RequireEmpresaId();

    public Task<List<Pacote>> ListarAsync() =>
        db.Pacotes
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Where(p => p.EmpresaId == EmpresaId && p.Ativo)
            .OrderBy(p => p.QuantidadeTotal)
            .ToListAsync();

    public Task<Pacote?> ObterAsync(int id) =>
        db.Pacotes
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == EmpresaId);

    public async Task SalvarAsync(Pacote pacote, List<PacoteItem> itens)
    {
        if (pacote.Id == 0)
        {
            pacote.EmpresaId = EmpresaId;
            pacote.Itens = itens;
            db.Pacotes.Add(pacote);
        }
        else
        {
            var existente = await db.Pacotes.Include(p => p.Itens)
                .FirstAsync(p => p.Id == pacote.Id && p.EmpresaId == EmpresaId);
            existente.Nome = pacote.Nome;
            existente.QuantidadeTotal = pacote.QuantidadeTotal;
            existente.Preco = pacote.Preco;
            existente.Ativo = pacote.Ativo;
            db.PacoteItens.RemoveRange(existente.Itens);
            foreach (var item in itens)
            {
                item.PacoteId = pacote.Id;
                db.PacoteItens.Add(item);
            }
        }
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var pacote = await db.Pacotes.FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == EmpresaId);
        if (pacote != null)
        {
            pacote.Ativo = false;
            await db.SaveChangesAsync();
        }
    }
}
