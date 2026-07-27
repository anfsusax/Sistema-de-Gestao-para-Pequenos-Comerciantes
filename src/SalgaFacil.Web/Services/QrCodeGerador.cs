using QRCoder;

namespace SalgaFacil.Web.Services;

/// <summary>
/// Renderiza um payload de texto (o BR Code gerado por
/// SalgaFacil.Domain.Services.PixPayloadGerador) como imagem PNG de QR Code. Usa a biblioteca
/// QRCoder (MIT, https://github.com/codebude/QRCoder) — só codifica texto em imagem, não fala
/// com nenhum serviço externo nem gateway de pagamento; é equivalente a uma lib de geração de
/// código de barras. Fica na camada Web (não no Domain) porque é a única peça deste fluxo com
/// dependência de biblioteca de imagem.
/// </summary>
public static class QrCodeGerador
{
    /// <summary>PNG em bytes do QR Code do <paramref name="payload"/> informado.</summary>
    public static byte[] GerarPng(string payload)
    {
        using var geradorDados = QRCodeGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(geradorDados);
        return qrCode.GetGraphic(10);
    }
}
