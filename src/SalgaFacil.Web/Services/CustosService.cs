using Microsoft.EntityFrameworkCore;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class CustosService(SalgaFacilDbContext db)
{
    public async Task<CustosDto> ObterAsync()
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var pedidosMes = await db.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Include(p => p.Itens).ThenInclude(i => i.Pacote).ThenInclude(p => p!.Itens).ThenInclude(pi => pi.Produto)
            .Where(p => p.Data >= inicioMes)
            .ToListAsync();

        var receita = pedidosMes.Sum(p => p.Total);
        decimal custo = 0;
        foreach (var pedido in pedidosMes)
        {
            foreach (var item in pedido.Itens)
            {
                if (item.Produto != null)
                    custo += item.Produto.CustoEstimado * item.Quantidade;
                else if (item.Pacote?.Itens != null)
                    custo += item.Pacote.Itens.Sum(pi => pi.Produto.CustoEstimado * pi.Quantidade) * item.Quantidade;
            }
        }

        var produtos = await db.Produtos.OrderBy(p => p.Nome).ToListAsync();
        var margens = produtos.Select(p => new MargemProduto
        {
            Nome = p.Nome,
            Tipo = p.Tipo,
            Custo = p.CustoEstimado,
            Preco = p.PrecoVenda,
            Lucro = p.PrecoVenda - p.CustoEstimado,
            Margem = p.PrecoVenda > 0 ? (int)Math.Round((p.PrecoVenda - p.CustoEstimado) / p.PrecoVenda * 100) : 0,
            Ativo = p.Ativo
        }).ToList();

        return new CustosDto
        {
            ReceitaMes = receita,
            CustoEstimadoMes = custo,
            LucroEstimadoMes = receita - custo,
            TotalPedidosMes = pedidosMes.Count,
            Margens = margens
        };
    }
}

public class CustosDto
{
    public decimal ReceitaMes { get; set; }
    public decimal CustoEstimadoMes { get; set; }
    public decimal LucroEstimadoMes { get; set; }
    public int TotalPedidosMes { get; set; }
    public int MargemPercentual => ReceitaMes > 0 ? (int)Math.Round(LucroEstimadoMes / ReceitaMes * 100) : 0;
    public List<MargemProduto> Margens { get; set; } = [];
}

public class MargemProduto
{
    public string Nome { get; set; } = string.Empty;
    public Domain.Enums.TipoProduto Tipo { get; set; }
    public decimal Custo { get; set; }
    public decimal Preco { get; set; }
    public decimal Lucro { get; set; }
    public int Margem { get; set; }
    public bool Ativo { get; set; }
}
