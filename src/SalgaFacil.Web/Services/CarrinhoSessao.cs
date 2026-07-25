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
///
/// IMPORTANTE (bug 1 corrigido em 2026-07-25 — "carrinho não atualiza, precisa recarregar"):
/// os métodos abaixo só capturavam <see cref="JSException"/>. Qualquer outra falha do JS
/// interop — em especial <see cref="JSDisconnectedException"/>, lançada quando o circuito
/// SignalR cai ou está reconectando no meio da chamada — escapava sem tratamento, subia pela
/// EventCallback do botão "Adicionar" e derrubava o circuito inteiro (o usuário via o
/// ReconnectModal/"Erro inesperado. Recarregar" do App.razor). Por isso agora capturamos
/// também <see cref="JSDisconnectedException"/> e <see cref="OperationCanceledException"/>,
/// sempre com log — a cópia em memória (`_porSlug`) já foi atualizada antes da chamada JS em
/// todos os pontos de entrada, então o carrinho desta sessão de circuito continua correto
/// mesmo quando a escrita no localStorage falha; só não sobrevive a um F5 nesse caso.
///
/// IMPORTANTE (bug 2 corrigido em 2026-07-25 — "só atualiza quando eu clico no carrinho"):
/// o item acima não era a causa raiz do sintoma relatado. LojaNav.razor aparece em
/// três páginas (Index, Carrinho, MeusPedidos) lendo <see cref="QuantidadeTotal"/> direto
/// deste serviço Scoped a cada render — mas nada avisava essas instâncias quando o carrinho
/// mudava em OUTRO componente da árvore (ex.: o modal de quantidade em Index.razor). O
/// contador só refletia o valor novo quando o próprio LojaNav renderizava de novo por conta
/// própria (ao navegar para /carrinho, que dispara um render inicial da página). Corrigido
/// com o evento <see cref="CarrinhoAlterado"/>: qualquer método que muda `_porSlug` notifica
/// os assinantes imediatamente, e cada LojaNav.razor se inscreve/desinscreve
/// (ver Dispose lá) para chamar o próprio StateHasChanged assim que o slug dele mudar —
/// independente de quem foi o componente que originou a mudança.
/// </summary>
public class CarrinhoSessao(IJSRuntime js, ILogger<CarrinhoSessao> logger)
{
    private readonly Dictionary<string, List<ItemCarrinhoDto>> _porSlug = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _carregados = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Disparado (com o slug afetado) sempre que o carrinho em memória muda: adicionar,
    /// alterar quantidade, remover ou esvaziar. Disparado ANTES da tentativa de persistir no
    /// localStorage, para que a UI atualize imediatamente mesmo que a escrita no storage
    /// demore ou falhe. Componentes que só leem <see cref="QuantidadeTotal"/> via injeção
    /// (não recebem o carrinho como [Parameter]) precisam se inscrever aqui para saber quando
    /// re-renderizar — ver LojaNav.razor.
    /// </summary>
    public event Action<string>? CarrinhoAlterado;

    /// <summary>
    /// Diagnóstico temporário (2026-07-25) — número de assinantes atuais de
    /// <see cref="CarrinhoAlterado"/>. Exposto para aparecer no toast da UI (ver Index.razor)
    /// porque o usuário não tem acesso fácil ao console/log do servidor. Remover junto com o
    /// resto da instrumentação quando o bug for confirmado corrigido.
    /// </summary>
    public int AssinantesCarrinhoAlterado => CarrinhoAlterado?.GetInvocationList().Length ?? 0;

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
        catch (JSException ex)
        {
            // localStorage indisponível (modo privado bloqueando storage, navegador antigo, etc.)
            // — carrinho simplesmente começa vazio nesta sessão, sem quebrar a página.
            logger.LogWarning(ex, "Falha ao ler carrinho do localStorage para slug {Slug}", slug);
        }
        catch (JsonException ex)
        {
            // Conteúdo salvo corrompido/de uma versão antiga incompatível — ignora e recomeça vazio
            // em vez de propagar erro para o cliente no meio da compra.
            logger.LogWarning(ex, "Carrinho salvo corrompido no localStorage para slug {Slug}", slug);
        }
        catch (JSDisconnectedException ex)
        {
            // Circuito caiu/está reconectando durante a chamada JS — não há UI para atualizar
            // mesmo, então só registra e segue (ver nota na doc da classe).
            logger.LogWarning(ex, "Circuito desconectado ao ler carrinho do localStorage para slug {Slug}", slug);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Leitura do carrinho no localStorage cancelada/expirou para slug {Slug}", slug);
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

        // Log temporário de diagnóstico (2026-07-25) — mostra quantos assinantes o evento tem
        // no momento do disparo: se vier "0 assinantes", o LojaNav.OnInitialized não está
        // rodando/inscrevendo (ou é outra instância de CarrinhoSessao); se vier "1 assinante" e
        // mesmo assim a UI não mudar, o problema é downstream (dentro de LojaNav ou no lado do
        // navegador), não neste evento.
        logger.LogInformation(
            "CarrinhoAlterado disparado para slug {Slug} — {Assinantes} assinante(s), total agora = {Total}",
            slug, CarrinhoAlterado?.GetInvocationList().Length ?? 0, QuantidadeTotal(slug));

        CarrinhoAlterado?.Invoke(slug);
        await SalvarAsync(slug);
    }

    public async Task LimparAsync(string slug)
    {
        _porSlug.Remove(slug);
        CarrinhoAlterado?.Invoke(slug);
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", Chave(slug));
        }
        catch (JSException ex)
        {
            // Mesmo motivo de CarregarAsync — não deve impedir a finalização do pedido.
            logger.LogWarning(ex, "Falha ao limpar carrinho no localStorage para slug {Slug}", slug);
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogWarning(ex, "Circuito desconectado ao limpar carrinho no localStorage para slug {Slug}", slug);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Limpeza do carrinho no localStorage cancelada/expirou para slug {Slug}", slug);
        }
    }

    private async Task SalvarAsync(string slug)
    {
        try
        {
            var json = JsonSerializer.Serialize(Obter(slug));
            await js.InvokeVoidAsync("localStorage.setItem", Chave(slug), json);
        }
        catch (JSException ex)
        {
            // Mesmo motivo de CarregarAsync — a cópia em memória (_porSlug) continua correta
            // para o resto desta sessão de circuito, só não sobrevive a um F5.
            logger.LogWarning(ex, "Falha ao salvar carrinho no localStorage para slug {Slug}", slug);
        }
        catch (JSDisconnectedException ex)
        {
            // Este é o caso que quebrava a UI antes da correção: sem este catch, a exceção
            // subia pela EventCallback do botão "Adicionar" (Index.razor -> ConfirmarAdicao)
            // e derrubava o circuito Blazor inteiro, forçando o usuário a recarregar a página
            // para o carrinho voltar a refletir o item recém-adicionado.
            logger.LogWarning(ex, "Circuito desconectado ao salvar carrinho no localStorage para slug {Slug}", slug);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Gravação do carrinho no localStorage cancelada/expirou para slug {Slug}", slug);
        }
    }

    public int QuantidadeTotal(string slug) => Obter(slug).Sum(i => i.Quantidade);
}
