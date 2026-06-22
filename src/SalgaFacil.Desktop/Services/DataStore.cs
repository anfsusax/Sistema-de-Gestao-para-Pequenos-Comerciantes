using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Models;

namespace SalgaFacil.Desktop.Services;

/// <summary>Repositório em memória com dados do protótipo HTML.</summary>
public static class DataStore
{
    public static Usuario? UsuarioLogado { get; set; }
    public static EmpresaConfig Empresa { get; set; } = new();
    public static List<Produto> Produtos { get; } = [];
    public static List<Cliente> Clientes { get; } = [];
    public static List<Pedido> Pedidos { get; } = [];
    public static List<ItemProducao> Producao { get; } = [];
    public static List<CustoProduto> Custos { get; } = [];
    public static List<EntregaDashboard> Entregas { get; } = [];

    private static int _proximoIdProduto = 5;
    private static int _proximoIdCliente = 5;
    private static int _proximoNumeroPedido = 48;

    public static void Inicializar()
    {
        if (Produtos.Count > 0) return;

        Produtos.AddRange([
            new Produto { Id = 1, Nome = "Coxinha", Categoria = "Salgado", Tipo = TipoProduto.Frito, PrecoVenda = 4.50m, CustoEstimado = 1.80m, Status = StatusProduto.Ativo },
            new Produto { Id = 2, Nome = "Esfiha", Categoria = "Salgado", Tipo = TipoProduto.Assado, PrecoVenda = 4.00m, CustoEstimado = 1.50m, Status = StatusProduto.Ativo },
            new Produto { Id = 3, Nome = "Risole", Categoria = "Salgado", Tipo = TipoProduto.Frito, PrecoVenda = 4.50m, CustoEstimado = 2.00m, Status = StatusProduto.Ativo },
            new Produto { Id = 4, Nome = "Empada", Categoria = "Salgado", Tipo = TipoProduto.Assado, PrecoVenda = 5.00m, CustoEstimado = 2.20m, Status = StatusProduto.Inativo }
        ]);

        Clientes.AddRange([
            new Cliente { Id = 1, Nome = "Maria Silva", Telefone = "(11) 99999-1234", TotalPedidos = 8, UltimaCompra = new DateTime(2026, 6, 20) },
            new Cliente { Id = 2, Nome = "João Padaria", Telefone = "(11) 98888-5678", TotalPedidos = 15, UltimaCompra = new DateTime(2026, 6, 21) },
            new Cliente { Id = 3, Nome = "Festa da Ana", Telefone = "(11) 97777-9012", TotalPedidos = 3, UltimaCompra = new DateTime(2026, 6, 21) },
            new Cliente { Id = 4, Nome = "Lanchonete Central", Telefone = "(11) 96666-3456", TotalPedidos = 22, UltimaCompra = new DateTime(2026, 6, 19) }
        ]);

        Pedidos.AddRange([
            new Pedido { Numero = 47, Cliente = "Festa da Ana", Data = new DateTime(2026, 6, 21), Valor = 850m, Status = StatusPedido.Pronto },
            new Pedido { Numero = 46, Cliente = "João Padaria", Data = new DateTime(2026, 6, 21), Valor = 320m, Status = StatusPedido.Aguardando },
            new Pedido { Numero = 45, Cliente = "Maria Silva", Data = new DateTime(2026, 6, 20), Valor = 480m, Status = StatusPedido.Produzindo },
            new Pedido { Numero = 44, Cliente = "Lanchonete Central", Data = new DateTime(2026, 6, 19), Valor = 1200m, Status = StatusPedido.Entregue }
        ]);

        Producao.AddRange([
            new ItemProducao { Pedido = "#0045", Produto = "Coxinha", Quantidade = 100, Tipo = TipoProduto.Frito, Status = StatusProducao.Produzindo },
            new ItemProducao { Pedido = "#0045", Produto = "Esfiha", Quantidade = 100, Tipo = TipoProduto.Assado, Status = StatusProducao.Produzindo },
            new ItemProducao { Pedido = "#0046", Produto = "Coxinha", Quantidade = 200, Tipo = TipoProduto.Frito, Status = StatusProducao.NaoIniciado },
            new ItemProducao { Pedido = "#0046", Produto = "Risole", Quantidade = 100, Tipo = TipoProduto.Frito, Status = StatusProducao.Finalizado }
        ]);

        Custos.AddRange([
            new CustoProduto { Produto = "Coxinha", CustoUnitario = 1.80m, PrecoVenda = 4.50m },
            new CustoProduto { Produto = "Esfiha", CustoUnitario = 1.50m, PrecoVenda = 4.00m },
            new CustoProduto { Produto = "Risole", CustoUnitario = 2.00m, PrecoVenda = 4.50m },
            new CustoProduto { Produto = "Empada", CustoUnitario = 2.20m, PrecoVenda = 5.00m }
        ]);

        Entregas.AddRange([
            new EntregaDashboard { Pedido = "#0045", Cliente = "Maria Silva", Entrega = "22/06 14:00", Status = StatusPedido.Produzindo },
            new EntregaDashboard { Pedido = "#0046", Cliente = "João Padaria", Entrega = "22/06 16:00", Status = StatusPedido.Aguardando },
            new EntregaDashboard { Pedido = "#0047", Cliente = "Festa da Ana", Entrega = "23/06 10:00", Status = StatusPedido.Pronto }
        ]);
    }

    public static int NovoIdProduto() => _proximoIdProduto++;
    public static int NovoIdCliente() => _proximoIdCliente++;
    public static int NovoNumeroPedido() => _proximoNumeroPedido++;
}
