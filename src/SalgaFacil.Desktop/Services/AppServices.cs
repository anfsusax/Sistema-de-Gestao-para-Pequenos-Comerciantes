using SalgaFacil.Desktop.Enums;
using SalgaFacil.Desktop.Models;

namespace SalgaFacil.Desktop.Services;

public class ProdutoService
{
    public List<Produto> Listar(string? tipoFiltro, bool apenasAtivos)
    {
        var q = DataStore.Produtos.AsEnumerable();
        if (apenasAtivos) q = q.Where(p => p.Status == StatusProduto.Ativo);
        if (tipoFiltro == "Fritos") q = q.Where(p => p.Tipo == TipoProduto.Frito);
        if (tipoFiltro == "Assados") q = q.Where(p => p.Tipo == TipoProduto.Assado);
        return q.ToList();
    }

    public void Salvar(Produto produto)
    {
        if (produto.Id == 0)
        {
            produto.Id = DataStore.NovoIdProduto();
            DataStore.Produtos.Add(produto);
        }
        else
        {
            var idx = DataStore.Produtos.FindIndex(p => p.Id == produto.Id);
            if (idx >= 0) DataStore.Produtos[idx] = produto;
        }
        AtualizarCustos();
    }

    public void Excluir(int id) => DataStore.Produtos.RemoveAll(p => p.Id == id);

    public Produto? Obter(int id) => DataStore.Produtos.FirstOrDefault(p => p.Id == id);

    private static void AtualizarCustos()
    {
        DataStore.Custos.Clear();
        foreach (var p in DataStore.Produtos)
            DataStore.Custos.Add(new CustoProduto { Produto = p.Nome, CustoUnitario = p.CustoEstimado, PrecoVenda = p.PrecoVenda });
    }
}

public class ClienteService
{
    public List<Cliente> Listar() => DataStore.Clientes.ToList();

    public void Salvar(Cliente cliente)
    {
        if (cliente.Id == 0)
        {
            cliente.Id = DataStore.NovoIdCliente();
            DataStore.Clientes.Add(cliente);
        }
        else
        {
            var idx = DataStore.Clientes.FindIndex(c => c.Id == cliente.Id);
            if (idx >= 0) DataStore.Clientes[idx] = cliente;
        }
    }

    public void Excluir(int id) => DataStore.Clientes.RemoveAll(c => c.Id == id);

    public Cliente? Obter(int id) => DataStore.Clientes.FirstOrDefault(c => c.Id == id);
}

public class PedidoService
{
    public List<Pedido> Listar(string? statusFiltro, DateTime? dataFiltro)
    {
        var q = DataStore.Pedidos.AsEnumerable();
        if (!string.IsNullOrEmpty(statusFiltro) && statusFiltro != "Todos")
        {
            StatusPedido? filtro = statusFiltro switch
            {
                "Aguardando" => StatusPedido.Aguardando,
                "Em produção" => StatusPedido.Produzindo,
                "Pronto" => StatusPedido.Pronto,
                "Entregue" => StatusPedido.Entregue,
                _ => null
            };
            if (filtro.HasValue) q = q.Where(p => p.Status == filtro.Value);
        }
        if (dataFiltro.HasValue)
            q = q.Where(p => p.Data.Date == dataFiltro.Value.Date);
        return q.OrderByDescending(p => p.Numero).ToList();
    }

    public void Salvar(Pedido pedido)
    {
        if (!DataStore.Pedidos.Any(p => p.Numero == pedido.Numero))
            DataStore.Pedidos.Insert(0, pedido);
    }

    public static string StatusLabel(StatusPedido s) => s switch
    {
        StatusPedido.Aguardando => "Aguardando",
        StatusPedido.Produzindo => "Produzindo",
        StatusPedido.Pronto => "Pronto",
        StatusPedido.Entregue => "Entregue",
        _ => s.ToString()
    };
}

public class ProducaoService
{
    public List<ItemProducao> Listar() => DataStore.Producao.ToList();

    public (int Total, int Fritos, int Assados) ObterTotais()
    {
        var itens = DataStore.Producao.Where(p => p.Status != StatusProducao.Finalizado).ToList();
        return (itens.Sum(i => i.Quantidade), itens.Where(i => i.Tipo == TipoProduto.Frito).Sum(i => i.Quantidade), itens.Where(i => i.Tipo == TipoProduto.Assado).Sum(i => i.Quantidade));
    }
}

public class CustoService
{
    public List<CustoProduto> Listar() => DataStore.Custos.ToList();
    public decimal CustoMensal => 3200m;
    public decimal VendaMensal => 8450m;
}

public class AuthService
{
    public bool Login(string email, string senha)
    {
        if (email.Equals("admin@salgapro.com", StringComparison.OrdinalIgnoreCase) && senha == "123456")
        {
            DataStore.UsuarioLogado = new Usuario { Nome = "Maria", Email = email, Senha = senha };
            return true;
        }
        return false;
    }
}
