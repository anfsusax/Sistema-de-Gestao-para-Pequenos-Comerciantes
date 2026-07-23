namespace SalgaFacil.Web.Services;

/// <summary>Carrinho em memória do circuito Blazor (área pública).</summary>
public class CarrinhoSessao
{
    private readonly Dictionary<string, List<ItemCarrinhoDto>> _porSlug = new(StringComparer.OrdinalIgnoreCase);

    public List<ItemCarrinhoDto> Obter(string slug)
    {
        if (!_porSlug.TryGetValue(slug, out var itens))
        {
            itens = [];
            _porSlug[slug] = itens;
        }
        return itens;
    }

    public void Definir(string slug, List<ItemCarrinhoDto> itens) =>
        _porSlug[slug] = itens.Select(i => new ItemCarrinhoDto { ProdutoId = i.ProdutoId, Quantidade = i.Quantidade }).ToList();

    public void Limpar(string slug) => _porSlug.Remove(slug);

    public int QuantidadeTotal(string slug) => Obter(slug).Sum(i => i.Quantidade);
}
