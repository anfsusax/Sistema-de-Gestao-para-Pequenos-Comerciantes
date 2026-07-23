using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class PedidoService(SalgaFacilDbContext db)
{
    public Task<List<Pedido>> ListarAsync(StatusPedido? status = null, string? busca = null) =>
        db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Include(p => p.Itens).ThenInclude(i => i.Pacote)
            .Where(p => !status.HasValue || p.Status == status.Value)
            .Where(p => busca == null || p.Cliente.Nome.Contains(busca) || p.Id.ToString().Contains(busca))
            .OrderByDescending(p => p.Data)
            .ToListAsync();

    public Task<Pedido?> ObterAsync(int id) =>
        db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Include(p => p.Itens).ThenInclude(i => i.Pacote)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> CriarAsync(int clienteId, List<NovoPedidoItem> itens)
    {
        var pedido = new Pedido { ClienteId = clienteId, Data = DateTime.UtcNow, Status = StatusPedido.Aguardando };
        foreach (var item in itens)
        {
            pedido.Itens.Add(new PedidoItem
            {
                ProdutoId = item.ProdutoId,
                PacoteId = item.PacoteId,
                Descricao = item.Descricao,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                Total = item.Quantidade * item.ValorUnitario
            });
        }
        pedido.Total = pedido.Itens.Sum(i => i.Total);
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        return pedido.Id;
    }

    /// <remarks>
    /// Estoque: um Pedido é produzido sob encomenda (não sai de um estoque fixo pré-existente,
    /// diferente da Venda de balcão), então o efeito no estoque acontece nas transições de
    /// status, não na criação: "Pronto" significa que o item foi fisicamente produzido (soma ao
    /// estoque); "Entregue" significa que o item saiu do estabelecimento (subtrai do estoque).
    /// Isso mantém um único saldo de estoque coerente entre Pedido e Venda — produção (própria
    /// ou excedente de um pedido) fica disponível para venda de balcão também.
    /// O efeito só é aplicado uma vez por transição (compara com o status anterior) para não
    /// duplicar caso o mesmo status seja setado de novo. Ver _ia/DECISOES.md.
    /// </remarks>
    public async Task AtualizarStatusAsync(int id, StatusPedido novoStatus)
    {
        var pedido = await db.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Include(p => p.Itens).ThenInclude(i => i.Pacote).ThenInclude(pac => pac!.Itens).ThenInclude(pi => pi.Produto)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null) return;

        var statusAnterior = pedido.Status;
        pedido.Status = novoStatus;

        if (novoStatus == StatusPedido.Pronto && statusAnterior != StatusPedido.Pronto)
            AjustarEstoquePorPedido(pedido, sinal: 1);
        else if (novoStatus == StatusPedido.Entregue && statusAnterior != StatusPedido.Entregue)
            AjustarEstoquePorPedido(pedido, sinal: -1);

        await db.SaveChangesAsync();
    }

    private static void AjustarEstoquePorPedido(Pedido pedido, int sinal)
    {
        foreach (var item in pedido.Itens)
        {
            if (item.Produto != null)
            {
                item.Produto.EstoqueAtual += sinal * item.Quantidade;
            }
            else if (item.Pacote?.Itens != null)
            {
                foreach (var pi in item.Pacote.Itens)
                    pi.Produto.EstoqueAtual += sinal * pi.Quantidade * item.Quantidade;
            }
        }
    }

    public async Task ExcluirAsync(int id)
    {
        var pedido = await db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null) return;
        db.PedidoItens.RemoveRange(pedido.Itens);
        db.Pedidos.Remove(pedido);
        await db.SaveChangesAsync();
    }

    public async Task<List<ProducaoPorProduto>> ObterProducaoPorProdutoAsync()
    {
        var pedidos = await db.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Include(p => p.Itens).ThenInclude(i => i.Pacote).ThenInclude(p => p!.Itens).ThenInclude(pi => pi.Produto)
            .Where(p => p.Status == StatusPedido.Aguardando || p.Status == StatusPedido.EmProducao)
            .ToListAsync();

        var mapa = new Dictionary<int, ProducaoPorProduto>();
        foreach (var pedido in pedidos)
        {
            foreach (var item in pedido.Itens)
            {
                if (item.ProdutoId.HasValue && item.Produto != null)
                    Adicionar(mapa, item.Produto, item.Quantidade, pedido.Id);
                else if (item.Pacote?.Itens != null)
                    foreach (var pi in item.Pacote.Itens)
                        Adicionar(mapa, pi.Produto, pi.Quantidade * item.Quantidade, pedido.Id);
            }
        }
        return mapa.Values.OrderByDescending(p => p.QuantidadeTotal).ToList();
    }

    private static void Adicionar(Dictionary<int, ProducaoPorProduto> mapa, Produto produto, int qtd, int pedidoId)
    {
        if (!mapa.TryGetValue(produto.Id, out var entry))
        {
            entry = new ProducaoPorProduto { ProdutoId = produto.Id, Nome = produto.Nome, QuantidadeTotal = 0, PedidosIds = [] };
            mapa[produto.Id] = entry;
        }
        entry.QuantidadeTotal += qtd;
        if (!entry.PedidosIds.Contains(pedidoId)) entry.PedidosIds.Add(pedidoId);
    }
}

public class NovoPedidoItem
{
    public int? ProdutoId { get; set; }
    public int? PacoteId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}

public class ProducaoPorProduto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeTotal { get; set; }
    public List<int> PedidosIds { get; set; } = [];
}
