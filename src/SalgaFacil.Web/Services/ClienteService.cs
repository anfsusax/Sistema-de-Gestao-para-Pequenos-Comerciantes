using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class ClienteService(SalgaFacilDbContext db)
{
    public Task<List<Cliente>> ListarAsync() =>
        db.Clientes.OrderBy(c => c.Nome).ToListAsync();

    public Task<List<ClienteComResumo>> ListarComResumoAsync() =>
        db.Clientes
            .Select(c => new ClienteComResumo
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                Endereco = c.Endereco,
                Observacoes = c.Observacoes,
                TotalPedidos = c.Pedidos.Count,
                UltimaCompra = c.Pedidos.OrderByDescending(p => p.Data).Select(p => (DateTime?)p.Data).FirstOrDefault()
            })
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public Task<Cliente?> ObterAsync(int id) => db.Clientes.FindAsync(id).AsTask();

    public async Task SalvarAsync(Cliente cliente)
    {
        if (cliente.Id == 0)
            db.Clientes.Add(cliente);
        else
            db.Clientes.Update(cliente);
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente != null)
        {
            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync();
        }
    }
}

public class ClienteComResumo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Observacoes { get; set; }
    public int TotalPedidos { get; set; }
    public DateTime? UltimaCompra { get; set; }
}
