namespace SalgaFacil.Tests.TestSupport;

/// <summary>Bytes mínimos válidos (cabeçalho/assinatura correta) para os formatos aceitos por ComprovanteArmazenamentoService, usados nos testes de upload.</summary>
public static class ArquivosDeTeste
{
    public static byte[] JpgValido() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];

    public static byte[] PngValido() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

    public static byte[] PdfValido() => "%PDF-1.4\n%comprovante de teste\n"u8.ToArray();

    /// <summary>Extensão .jpg mas conteúdo de PDF — simula um arquivo malicioso disfarçado de imagem.</summary>
    public static byte[] JpgFalsificado() => PdfValido();

    public static byte[] MaiorQueLimite() => new byte[ComprovanteArmazenamentoServiceLimiteBytes + 1];

    public const long ComprovanteArmazenamentoServiceLimiteBytes = 8 * 1024 * 1024;
}
