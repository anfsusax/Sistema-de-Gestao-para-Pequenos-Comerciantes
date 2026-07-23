using Microsoft.EntityFrameworkCore;
using SalgaFacil.Domain.Enums;
using SalgaFacil.Infrastructure.Data;

namespace SalgaFacil.Web.Services;

public class DashboardService(SalgaFacilDbContext db, IEmpresaContext empresa)
{
    public async Task<DashboardDto> ObterAsync()
    {
        var empresaId = empresa.RequireEmpresaId();
        var hoje = DateTime.UtcNow.Date;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek + (int)DayOfWeek.Monday);
        if (hoje.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoje.AddDays(-6);

        var pedidos = await db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .Where(p => p.EmpresaId == empresaId)
            .ToListAsync();
        var pedidosHoje = pedidos.Where(p => p.Data.Date == hoje).ToList();
        var pedidosAtivos = pedidos.Where(p => p.Status is StatusPedido.Aguardando or StatusPedido.EmProducao or StatusPedido.Pronto).ToList();

        var pedidosPorDia = Enumerable.Range(0, 7)
            .Select(i => inicioSemana.AddDays(i))
            .Select(d => new PedidoPorDia { Dia = d, Quantidade = pedidos.Count(p => p.Data.Date == d.Date) })
            .ToList();

        var produtosVendidos = pedidos
            .SelectMany(p => p.Itens)
            .Where(i => i.ProdutoId.HasValue && i.Produto != null)
            .GroupBy(i => i.Produto!.Nome)
            .Select(g => new ProdutoVendido { Nome = g.Key, Quantidade = g.Sum(i => i.Quantidade) })
            .OrderByDescending(p => p.Quantidade)
            .Take(4)
            .ToList();

        var totalVendido = produtosVendidos.Sum(p => p.Quantidade);
        foreach (var p in produtosVendidos)
            p.Percentual = totalVendido > 0 ? (int)Math.Round(p.Quantidade * 100m / totalVendido) : 0;

        return new DashboardDto
        {
            PedidosHoje = pedidosHoje.Count,
            PedidosTotalAtivos = pedidosAtivos.Count,
            EmProducao = pedidos.Count(p => p.Status == StatusPedido.EmProducao),
            Aguardando = pedidos.Count(p => p.Status == StatusPedido.Aguardando),
            ReceitaHoje = pedidosHoje.Sum(p => p.Total),
            ReceitaSemana = pedidos.Where(p => p.Data.Date >= inicioSemana && p.Data.Date <= hoje).Sum(p => p.Total),
            PedidosPorDia = pedidosPorDia,
            ProdutosMaisVendidos = produtosVendidos,
            ProximasEntregas = pedidos
                .Where(p => p.Status is StatusPedido.Aguardando or StatusPedido.EmProducao or StatusPedido.Pronto)
                .OrderBy(p => p.DataEntregaPrevista ?? p.Data)
                .Take(5)
                .Select(p => new ProximaEntrega
                {
                    PedidoId = p.Id,
                    ClienteNome = p.Cliente.Nome,
                    ItensResumo = string.Join(" · ", p.Itens.Select(i => $"{i.Quantidade}x {i.Descricao}")),
                    Valor = p.Total,
                    Status = p.Status
                })
                .ToList()
        };
    }
}

public class DashboardDto
{
    public int PedidosHoje { get; set; }
    public int PedidosTotalAtivos { get; set; }
    public int EmProducao { get; set; }
    public int Aguardando { get; set; }
    public decimal ReceitaHoje { get; set; }
    public decimal ReceitaSemana { get; set; }
    public List<PedidoPorDia> PedidosPorDia { get; set; } = [];
    public List<ProdutoVendido> ProdutosMaisVendidos { get; set; } = [];
    public List<ProximaEntrega> ProximasEntregas { get; set; } = [];
}

public class PedidoPorDia
{
    public DateTime Dia { get; set; }
    public int Quantidade { get; set; }
    public string Label => Dia.ToString("ddd", new System.Globalization.CultureInfo("pt-BR")).Substring(0, 3);
}

public class ProdutoVendido
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int Percentual { get; set; }
}

public class ProximaEntrega
{
    public int PedidoId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ItensResumo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public StatusPedido Status { get; set; }
}
