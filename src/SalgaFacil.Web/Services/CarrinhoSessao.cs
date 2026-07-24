using System.Text.Json;
using Microsoft.JSInterop;

namespace SalgaFacil.Web.Services;

/// <summary>
/// Carrinho do cardápio público. Guardado no localStorage do navegador (por slug de loja) —
/// sobrevive a F5, fechar/reabrir a aba e reconexão do circuito Blazor Server. Não é
/// compartilhado entre dispositivos/navegadores diferentes (ver _ia/DECISOES.md 2026-07-24,
/// "Carrinho Persistente Via LocalStorage" para a discussão de trade-offs).
///
/// Continua Scoped por circuito: mantém uma cópia em memória (`_porSlug`) para leitura
/// síncrona rápida durante a renderização, e sincroniza com o localStorage via JS interop
/// a cada mudança. JS interop só funciona depois que o circuito está interativo — por isso
/// <see cref="CarregarAsync"/> deve ser chamado a partir de OnAfterRenderAsync(firstRender),
/// nunca de OnInitializedAsync (que roda também durante o prerender estático, sem JS disponível).
/// </summary>
public class CarrinhoSessao(IJSRuntime js)
{
    private readonly Dictionary<string, List<ItemCarrinhoDto>> _porSlug = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _carregados = new(StringComparer.OrdinalIgnoreCase);

    private static string Chave(string slug) => $"salgafacil_carrinho_{slug.Trim().ToLowerInvariant()}";

    /// <summary>
    /// Carrega o carrinho salvo no localStorage para este slug, se ainda não carregado nesta
    /// sessão de circuito. Idempotente — chamadas repetidas para o mesmo slug não fazem nada.
    /// </summary>
    public async Task CarregarAsync(string slug)
    {
        if (_carregados.Contains(slug))
            return;
        _carregados.Add(slug);

        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", Chave(slug));
            if (string.IsNullOrWhiteSpace(json))
                return;

            var itens = JsonSerializer.Deserialize<List<ItemCarrinhoDto>>(json);
            if (itens is not null)
                _porSlug[slug] = itens;
        }
        catch (JSException)
        {
            // localStorage indisponível (modo privado bloqueando storage, navegador antigo, etc.)
            // — carrinho simplesmente começa vazio nesta sessão, sem quebrar a página.
        }
        catch (JsonException)
        {
            // Conteúdo salvo corrompido/de uma versão antiga incompatível — ignora e recomeça vazio
            // em vez de propagar erro para o cliente no meio da compra.
        }
    }

    public List<ItemCarrinhoDto> Obter(string slug)
    {
        if (!_porSlug.TryGetValue(slug, out var itens))
        {
            itens = [];
            _porSlug[slug] = itens;
        }
        return itens;
    }

    public async Task DefinirAsync(string slug, List<ItemCarrinhoDto> itens)
    {
        _porSlug[slug] = itens.Select(i => new ItemCarrinhoDto { ProdutoId = i.ProdutoId, Quantidade = i.Quantidade }).ToList();
        await SalvarAsync(slug);
    }

    public async Task LimparAsync(string slug)
    {
        _porSlug.Remove(slug);
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", Chave(slug));
        }
        catch (JSException)
        {
            // Mesmo motivo de CarregarAsync — não deve impedir a finalização do pedido.
        }
    }

    private async Task SalvarAsync(string slug)
    {
        try
        {
            var json = JsonSerializer.Serialize(Obter(slug));
            await js.InvokeVoidAsync("localStorage.setItem", Chave(slug), json);
        }
        catch (JSException)
        {
            // Mesmo motivo de CarregarAsync — a cópia em memória (_porSlug) continua correta
            // para o resto desta sessão de circuito, só não sobrevive a um F5.
        }
    }

    public int QuantidadeTotal(string slug) => Obter(slug).Sum(i => i.Quantidade);
}
