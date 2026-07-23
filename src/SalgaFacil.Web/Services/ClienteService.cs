using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class ClienteService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    private int EmpresaId => empresa.RequireEmpresaId();

    public Task<List<Cliente>> ListarAsync(bool? apenasAtivos = null) =>
        db.Clientes
            .Include(c => c.Enderecos)
            .Where(c => c.EmpresaId == EmpresaId)
            .Where(c => !apenasAtivos.HasValue || c.Ativo == apenasAtivos.Value)
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public Task<List<ClienteComResumo>> ListarComResumoAsync(string? busca = null) =>
        db.Clientes
            .Include(c => c.Enderecos)
            .Where(c => c.EmpresaId == EmpresaId)
            .Where(c => string.IsNullOrWhiteSpace(busca)
                || c.Nome.Contains(busca)
                || c.Telefone.Contains(busca)
                || (c.WhatsApp != null && c.WhatsApp.Contains(busca))
                || (c.Cpf != null && c.Cpf.Contains(busca)))
            .Select(c => new ClienteComResumo
            {
                Id = c.Id,
                Nome = c.Nome,
                Telefone = c.Telefone,
                WhatsApp = c.WhatsApp,
                Email = c.Email,
                Ativo = c.Ativo,
                Cidade = c.Enderecos.Where(e => e.Principal).Select(e => e.Cidade).FirstOrDefault()
                    ?? c.Enderecos.Select(e => e.Cidade).FirstOrDefault(),
                TotalPedidos = c.Pedidos.Count,
                UltimaCompra = c.Pedidos.OrderByDescending(p => p.Data).Select(p => (DateTime?)p.Data).FirstOrDefault()
            })
            .OrderBy(c => c.Nome)
            .ToListAsync();

    public Task<Cliente?> ObterAsync(int id) =>
        db.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId);

    public async Task SalvarAsync(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome))
            throw new InvalidOperationException("Nome do cliente é obrigatório.");

        cliente.Nome = cliente.Nome.Trim();
        if (!string.IsNullOrWhiteSpace(cliente.Email) && !cliente.Email.Contains('@'))
            throw new InvalidOperationException("E-mail inválido.");

        var principais = cliente.Enderecos.Count(e => e.Principal);
        if (principais > 1)
            throw new InvalidOperationException("Apenas um endereço pode ser o principal.");
        if (cliente.Enderecos.Count > 0 && principais == 0)
            cliente.Enderecos.First().Principal = true;

        if (cliente.Id == 0)
        {
            cliente.EmpresaId = EmpresaId;
            cliente.CriadoEm = DateTime.UtcNow;
            db.Clientes.Add(cliente);
        }
        else
        {
            var existente = await db.Clientes
                .Include(c => c.Enderecos)
                .FirstOrDefaultAsync(c => c.Id == cliente.Id && c.EmpresaId == EmpresaId)
                ?? throw new InvalidOperationException("Cliente não encontrado.");

            existente.Nome = cliente.Nome;
            existente.Cpf = cliente.Cpf;
            existente.Cnpj = cliente.Cnpj;
            existente.Telefone = cliente.Telefone;
            existente.WhatsApp = cliente.WhatsApp;
            existente.Email = cliente.Email;
            existente.DataNascimento = cliente.DataNascimento;
            existente.Observacoes = cliente.Observacoes;
            existente.Ativo = cliente.Ativo;
            existente.AtualizadoEm = DateTime.UtcNow;

            db.EnderecosCliente.RemoveRange(existente.Enderecos);
            foreach (var end in cliente.Enderecos)
            {
                end.Id = 0;
                end.ClienteId = existente.Id;
                existente.Enderecos.Add(end);
            }

            await db.SaveChangesAsync();
            return;
        }

        await db.SaveChangesAsync();
    }

    public async Task AlternarAtivoAsync(int id)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");
        cliente.Ativo = !cliente.Ativo;
        cliente.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == EmpresaId);
        if (cliente is null) return;

        var temPedido = await db.Pedidos.AnyAsync(p => p.ClienteId == id && p.EmpresaId == EmpresaId);
        if (temPedido)
            throw new InvalidOperationException("Não é possível excluir: cliente possui pedidos. Inative-o.");

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync();
    }
}

public class ClienteComResumo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Cidade { get; set; }
    public bool Ativo { get; set; }
    public int TotalPedidos { get; set; }
    public DateTime? UltimaCompra { get; set; }
}
