using SalgaFacil.Domain.Entities;

namespace SalgaFacil.Domain.Services;

/// <summary>
/// Ponto único de cálculo de preço por quantidade de um produto.
/// Usado tanto pela UI (pré-visualização do total no seletor de quantidade)
/// quanto pelo backend (cálculo autoritativo ao gravar o pedido) — evita que
/// as duas pontas calculem o total de formas diferentes.
///
/// Regra atual: preço único (<see cref="Produto.PrecoVenda"/>), independente
/// da quantidade. Preparado para evoluir para preço por faixa de quantidade
/// (ex.: 1un = R$1,50, 50un = R$1,30 cada, 100un = R$1,10 cada) sem exigir
/// mudança nos chamadores: quando o Domain ganhar uma tabela de faixas de
/// preço por produto, a alteração fica isolada em <see cref="PrecoUnitario"/>.
/// </summary>
public static class PrecificacaoProduto
{
    /// <summary>Quantidade mínima permitida para qualquer item de pedido.</summary>
    public const int QuantidadeMinima = 1;

    public static decimal CalcularTotal(Produto produto, int quantidade)
    {
        ValidarQuantidade(quantidade);
        return PrecoUnitario(produto, quantidade) * quantidade;
    }

    /// <summary>Preço unitário aplicável para a quantidade informada.</summary>
    public static decimal PrecoUnitario(Produto produto, int quantidade)
    {
        ValidarQuantidade(quantidade);

        // Regra atual: preço único, independente da quantidade.
        // Evolução futura (faixas de preço): consultar as faixas cadastradas
        // para o produto e retornar o preço da faixa com a maior
        // QuantidadeMinima que ainda seja <= quantidade. Ex.:
        //   var faixa = produto.FaixasPreco
        //       .Where(f => quantidade >= f.QuantidadeMinima)
        //       .OrderByDescending(f => f.QuantidadeMinima)
        //       .FirstOrDefault();
        //   return faixa?.PrecoUnitario ?? produto.PrecoVenda;
        return produto.PrecoVenda;
    }

    /// <summary>Valida a quantidade de acordo com as regras de negócio (inteiro >= 1).</summary>
    public static void ValidarQuantidade(int quantidade)
    {
        if (quantidade < QuantidadeMinima)
            throw new ArgumentOutOfRangeException(nameof(quantidade), $"Quantidade deve ser maior ou igual a {QuantidadeMinima}.");
    }
}
