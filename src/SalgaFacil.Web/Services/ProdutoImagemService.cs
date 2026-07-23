namespace SalgaFacil.Web.Services;

public class ProdutoImagemService(IWebHostEnvironment env, ILogger<ProdutoImagemService> logger)
{
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private string PastaUploads => Path.Combine(env.WebRootPath, "uploads", "produtos");

    public async Task<string> SalvarAsync(byte[] conteudo, string nomeOriginal, string? fotoAnterior = null)
    {
        if (conteudo.Length == 0)
            throw new InvalidOperationException("Arquivo de imagem inválido.");
        if (conteudo.Length > TamanhoMaximoBytes)
            throw new InvalidOperationException("A imagem deve ter no máximo 5 MB.");

        var extensao = Path.GetExtension(nomeOriginal);
        if (string.IsNullOrWhiteSpace(extensao) || !ExtensoesPermitidas.Contains(extensao))
            throw new InvalidOperationException("Formato não suportado. Use JPG, PNG, WEBP ou GIF.");

        Directory.CreateDirectory(PastaUploads);

        var nomeArquivo = $"{Guid.NewGuid():N}{extensao.ToLowerInvariant()}";
        var caminhoFisico = Path.Combine(PastaUploads, nomeArquivo);
        await File.WriteAllBytesAsync(caminhoFisico, conteudo);

        RemoverArquivoLocal(fotoAnterior);
        return $"/uploads/produtos/{nomeArquivo}";
    }

    public void RemoverArquivoLocal(string? fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(fotoUrl) || !fotoUrl.StartsWith("/uploads/produtos/", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            var nome = Path.GetFileName(fotoUrl);
            if (string.IsNullOrWhiteSpace(nome)) return;

            var caminho = Path.Combine(PastaUploads, nome);
            if (File.Exists(caminho))
                File.Delete(caminho);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível remover imagem antiga: {FotoUrl}", fotoUrl);
        }
    }
}
