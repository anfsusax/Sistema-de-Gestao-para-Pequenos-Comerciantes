using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Entities;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

/// <summary>Serviços públicos da loja (sem login administrativo).</summary>
public class LojaPublicaService(SalgaFacilDbContext db)
{
    public Task<Empresa?> ObterEmpresaPorSlugAsync(string slug) =>
        db.Empresas.FirstOrDefaultAsync(e => e.Slug == slug.Trim().ToLowerInvariant() && e.Ativo);

    public async Task<List<CategoriaProduto>> ListarCategoriasAsync(int empresaId) =>
        await db.CategoriasProduto
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.Ordem).ThenBy(c => c.Nome)
            .ToListAsync();

    public async Task<List<Produto>> ListarProdutosAsync(int empresaId, int? categoriaId = null) =>
        await db.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.UnidadeMedida)
            .Where(p => p.EmpresaId == empresaId && p.Ativo)
            .Where(p => !categoriaId.HasValue || p.CategoriaId == categoriaId.Value)
            .OrderBy(p => p.Categoria.Ordem).ThenBy(p => p.Nome)
            .ToListAsync();

    public async Task<int> CriarPedidoVisitanteAsync(int empresaId, PedidoVisitanteDto dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            throw new InvalidOperationException("Informe seu nome.");
        if (string.IsNullOrWhiteSpace(dados.Telefone) && string.IsNullOrWhiteSpace(dados.WhatsApp))
            throw new InvalidOperationException("Informe telefone ou WhatsApp.");
        if (dados.Itens.Count == 0)
            throw new InvalidOperationException("Carrinho vazio.");

        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId && e.Ativo)
            ?? throw new InvalidOperationException("Loja não encontrada.");

        var produtoIds = dados.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await db.Produtos
            .Where(p => p.EmpresaId == empresaId && p.Ativo && produtoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (produtos.Count != produtoIds.Count)
            throw new InvalidOperationException("Um ou mais produtos não estão disponíveis.");

        var cliente = new Cliente
        {
            EmpresaId = empresaId,
            Nome = dados.Nome.Trim(),
            Telefone = dados.Telefone?.Trim() ?? dados.WhatsApp!.Trim(),
            WhatsApp = dados.WhatsApp?.Trim() ?? dados.Telefone?.Trim(),
            Observacoes = "Pedido via cardápio público",
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
        if (dados.Entrega && !string.IsNullOrWhiteSpace(dados.EnderecoEntrega))
        {
            cliente.Enderecos.Add(new EnderecoCliente
            {
                Logradouro = dados.EnderecoEntrega.Trim(),
                Cidade = "—",
                Estado = "SP",
                Principal = true
            });
        }
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var pedido = new Pedido
        {
            EmpresaId = empresaId,
            ClienteId = cliente.Id,
            Data = DateTime.UtcNow,
            Status = StatusPedido.Aguardando,
            Entrega = dados.Entrega,
            EnderecoEntrega = dados.EnderecoEntrega,
            Observacoes = dados.Observacoes
        };

        foreach (var item in dados.Itens)
        {
            var produto = produtos[item.ProdutoId];
            pedido.Itens.Add(new PedidoItem
            {
                ProdutoId = produto.Id,
                Descricao = produto.Nome,
                Quantidade = item.Quantidade,
                ValorUnitario = produto.PrecoVenda,
                Total = item.Quantidade * produto.PrecoVenda
            });
        }
        pedido.Total = pedido.Itens.Sum(i => i.Total);
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        return pedido.Id;
    }
}

public class PedidoVisitanteDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public bool Entrega { get; set; }
    public string? EnderecoEntrega { get; set; }
    public string? Observacoes { get; set; }
    public List<ItemCarrinhoDto> Itens { get; set; } = [];
}

public class ItemCarrinhoDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
