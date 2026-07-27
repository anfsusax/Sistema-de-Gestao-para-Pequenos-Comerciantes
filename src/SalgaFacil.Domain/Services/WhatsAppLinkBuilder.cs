namespace SalgaFacil.Domain.Services;

/// <summary>
/// Monta o link "https://wa.me/&lt;numero&gt;?text=..." para o cliente falar com a loja sobre um
/// pedido específico (PIX-MANUAL-002, item 17 do contrato: "botão auxiliar para falar com a
/// loja pelo WhatsApp, enviando uma mensagem com o número do pedido"). Reaproveita
/// <see cref="TelefoneNormalizador"/> — mesmo formato "dígitos com DDI 55" que o wa.me espera.
/// </summary>
public static class WhatsAppLinkBuilder
{
    /// <summary>
    /// Retorna null se a loja não tiver WhatsApp cadastrado (ou não normalizar para um número
    /// válido) — quem chama simplesmente não mostra o botão nesse caso.
    /// </summary>
    public static string? MontarLinkPedido(string? whatsappLoja, int pedidoId)
    {
        var numero = TelefoneNormalizador.Normalizar(whatsappLoja);
        if (numero is null)
            return null;

        var mensagem = Uri.EscapeDataString($"Olá! Sobre o meu pedido #{pedidoId:D4}.");
        return $"https://wa.me/{numero}?text={mensagem}";
    }
}
