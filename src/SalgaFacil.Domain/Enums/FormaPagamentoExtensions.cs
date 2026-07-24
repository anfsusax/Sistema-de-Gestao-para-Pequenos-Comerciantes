namespace SalgaFacil.Domain.Enums;

/// <summary>
/// Rótulo em pt-BR para exibição de <see cref="FormaPagamento"/>. Ponto único usado pelo
/// checkout e histórico do cardápio público (Carrinho.razor, MeusPedidos.razor) — evita
/// repetir o mesmo switch em cada tela nova que precisar exibir a forma de pagamento.
/// Nota: Pdv/Index.razor e Pdv/Historico.razor têm switches equivalentes próprios, de antes
/// desta funcionalidade existir; não foram tocados aqui para não alterar código fora do
/// escopo desta mudança (ver _ia/DECISOES.md 2026-07-24).
/// </summary>
public static class FormaPagamentoExtensions
{
    public static string Rotulo(this FormaPagamento forma) => forma switch
    {
        FormaPagamento.Dinheiro => "Dinheiro",
        FormaPagamento.Pix => "Pix",
        FormaPagamento.CartaoDebito => "Cartão Débito",
        FormaPagamento.CartaoCredito => "Cartão Crédito",
        _ => forma.ToString()
    };

    /// <summary>
    /// Ícone (emoji) associado à forma de pagamento, para exibição em cards de seleção
    /// (ex.: Loja/Carrinho.razor). Mesmo motivo de centralização do <see cref="Rotulo"/>.
    /// </summary>
    public static string Icone(this FormaPagamento forma) => forma switch
    {
        FormaPagamento.Dinheiro => "💵",
        FormaPagamento.Pix => "🔷",
        FormaPagamento.CartaoDebito => "💳",
        FormaPagamento.CartaoCredito => "💳",
        _ => "💰"
    };
}
