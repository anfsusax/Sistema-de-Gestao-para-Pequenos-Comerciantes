namespace SalgaFacil.Web.Services;

/// <summary>
/// Armazenamento privado de comprovantes de pagamento Pix (PIX-MANUAL-002). Ao contrário de
/// <see cref="ProdutoImagemService"/> (que grava em wwwroot/uploads — servido publicamente por
/// app.MapStaticAssets), este serviço grava em ContentRootPath/App_Data/comprovantes, uma pasta
/// FORA de wwwroot. Não existe rota estática apontando para lá, e o caminho retornado nunca é
/// exposto ao cliente — só usado internamente para localizar o arquivo de novo.
///
/// A entrega ao navegador (abrir/baixar) é feita inteiramente dentro do circuito Blazor já
/// autenticado (bytes lidos aqui, devolvidos ao componente Razor, embutidos como
/// data URI/blob no cliente) — não existe endpoint HTTP separado para o comprovante, porque um
/// endpoint minimal API não teria acesso a AuthService/ClienteAuthService (sessão vive no
/// circuito, não em cookie/token HTTP; ver ESTADO.md "Persistência de autenticação limitada ao
/// circuito"). Isso elimina de saída a categoria de bug "esqueci de checar auth no endpoint".
/// </summary>
public class ComprovanteArmazenamentoService(IWebHostEnvironment env, ILogger<ComprovanteArmazenamentoService> logger)
{
    public const long TamanhoMaximoBytes = 8 * 1024 * 1024; // 8 MB — abaixo do limite de mensagem SignalR (10 MB, ver Program.cs)

    private static readonly Dictionary<string, byte[]> AssinaturasPorExtensao = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = [0xFF, 0xD8, 0xFF],
        [".jpeg"] = [0xFF, 0xD8, 0xFF],
        [".png"] = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
        [".pdf"] = "%PDF-"u8.ToArray()
    };

    private static readonly Dictionary<string, string> ContentTypePorExtensao = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".pdf"] = "application/pdf"
    };

    private string PastaComprovantes => Path.Combine(env.ContentRootPath, "App_Data", "comprovantes");

    /// <summary>
    /// Valida extensão, tamanho e assinatura binária (magic bytes) do arquivo — pega tanto um
    /// upload legítimo do tipo errado quanto uma tentativa de disfarçar outro tipo de arquivo
    /// (ex.: um .exe renomeado para .jpg tem os bytes iniciais errados e é recusado aqui, mesmo
    /// que o nome/extensão declarados pareçam válidos). Lança InvalidOperationException com
    /// mensagem segura para exibir ao cliente.
    /// </summary>
    public void Validar(byte[] conteudo, string nomeArquivoOriginal)
    {
        if (conteudo.Length == 0)
            throw new InvalidOperationException("Arquivo vazio.");
        if (conteudo.Length > TamanhoMaximoBytes)
            throw new InvalidOperationException($"O comprovante deve ter no máximo {TamanhoMaximoBytes / 1024 / 1024} MB.");

        var extensao = Path.GetExtension(nomeArquivoOriginal);
        if (string.IsNullOrWhiteSpace(extensao) || !AssinaturasPorExtensao.TryGetValue(extensao, out var assinatura))
            throw new InvalidOperationException("Formato não suportado. Envie um arquivo JPG, PNG ou PDF.");

        if (conteudo.Length < assinatura.Length || !conteudo.AsSpan(0, assinatura.Length).SequenceEqual(assinatura))
            throw new InvalidOperationException("O conteúdo do arquivo não corresponde a um JPG, PNG ou PDF válido.");
    }

    /// <summary>Content-Type correto para a extensão (ignora o que o navegador declarou — deriva do que foi validado).</summary>
    public static string ContentTypePara(string nomeArquivoOriginal) =>
        ContentTypePorExtensao.TryGetValue(Path.GetExtension(nomeArquivoOriginal), out var ct) ? ct : "application/octet-stream";

    /// <summary>
    /// Salva o arquivo já validado e retorna a chave interna (relativa à pasta de comprovantes,
    /// nunca uma URL) a persistir em Pedido.ComprovanteCaminho. Remove o arquivo anterior deste
    /// pedido, se houver (reenvio substitui, não acumula).
    /// </summary>
    public async Task<string> SalvarAsync(int empresaId, int pedidoId, byte[] conteudo, string nomeArquivoOriginal, string? caminhoAnterior)
    {
        Validar(conteudo, nomeArquivoOriginal);

        var pastaEmpresa = Path.Combine(PastaComprovantes, empresaId.ToString());
        Directory.CreateDirectory(pastaEmpresa);

        var extensao = Path.GetExtension(nomeArquivoOriginal).ToLowerInvariant();
        var nomeArquivo = $"pedido{pedidoId}_{Guid.NewGuid():N}{extensao}";
        var caminhoFisico = Path.Combine(pastaEmpresa, nomeArquivo);
        await File.WriteAllBytesAsync(caminhoFisico, conteudo);

        RemoverArquivo(empresaId, caminhoAnterior);

        // Chave interna: "<empresaId>/<nomeArquivo>" — nunca um caminho absoluto, nunca servido via URL.
        return $"{empresaId}/{nomeArquivo}";
    }

    public async Task<byte[]?> LerAsync(string? caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            return null;

        var caminhoFisico = ResolverCaminhoSeguro(caminho);
        if (caminhoFisico is null || !File.Exists(caminhoFisico))
            return null;

        return await File.ReadAllBytesAsync(caminhoFisico);
    }

    public void RemoverArquivo(int empresaId, string? caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            return;

        try
        {
            var caminhoFisico = ResolverCaminhoSeguro(caminho);
            if (caminhoFisico is not null && File.Exists(caminhoFisico))
                File.Delete(caminhoFisico);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível remover comprovante anterior: {Caminho}", caminho);
        }
    }

    /// <summary>
    /// Resolve a chave interna para um caminho físico, garantindo que o resultado continua
    /// DENTRO de PastaComprovantes — bloqueia qualquer tentativa de path traversal (ex.: uma
    /// chave manipulada contendo "../../"). Retorna null se a chave for inválida ou escapar da
    /// pasta base, em vez de lançar, para os chamadores tratarem como "arquivo não encontrado".
    /// </summary>
    private string? ResolverCaminhoSeguro(string chave)
    {
        var baseCompleta = Path.GetFullPath(PastaComprovantes);
        var caminhoCompleto = Path.GetFullPath(Path.Combine(PastaComprovantes, chave));

        return caminhoCompleto.StartsWith(baseCompleta + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? caminhoCompleto
            : null;
    }
}
