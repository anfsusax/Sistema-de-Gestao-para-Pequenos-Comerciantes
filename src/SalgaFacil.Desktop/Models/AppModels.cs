using SalgaFacil.Desktop.Enums;

namespace SalgaFacil.Desktop.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Salgado";
    public TipoProduto Tipo { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal CustoEstimado { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public StatusProduto Status { get; set; } = StatusProduto.Ativo;
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Observacao { get; set; } = string.Empty;
    public int TotalPedidos { get; set; }
    public DateTime UltimaCompra { get; set; }
}

public class ItemPedido
{
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Total => Quantidade * ValorUnitario;
}

public class Pedido
{
    public int Numero { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public DateTime? DataEntrega { get; set; }
    public decimal Valor { get; set; }
    public StatusPedido Status { get; set; }
    public List<ItemPedido> Itens { get; set; } = [];
}

public class ItemProducao
{
    public string Pedido { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public TipoProduto Tipo { get; set; }
    public StatusProducao Status { get; set; }
}

public class CustoProduto
{
    public string Produto { get; set; } = string.Empty;
    public decimal CustoUnitario { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal LucroEstimado => PrecoVenda - CustoUnitario;
    public int MargemPercentual => PrecoVenda > 0 ? (int)Math.Round(LucroEstimado / PrecoVenda * 100) : 0;
}

public class Usuario
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class EmpresaConfig
{
    public string Nome { get; set; } = "Salgados da Maria";
    public string Cnpj { get; set; } = "12.345.678/0001-99";
    public string Telefone { get; set; } = "(11) 99999-0000";
}

public class EntregaDashboard
{
    public string Pedido { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Entrega { get; set; } = string.Empty;
    public StatusPedido Status { get; set; }
}
