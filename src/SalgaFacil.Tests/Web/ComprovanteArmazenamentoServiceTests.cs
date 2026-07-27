using Microsoft.Extensions.Logging.Abstractions;
using SalgaFacil.Tests.TestSupport;
using SalgaFacil.Web.Services;
using Xunit;

namespace SalgaFacil.Tests.Web;

/// <summary>
/// PIX-MANUAL-002: cobre validação de upload (extensão/assinatura/tamanho — item "validar
/// tipo/extensão/assinatura/tamanho de arquivo" do contrato) e a proteção contra path traversal
/// no armazenamento privado do comprovante.
/// </summary>
public sealed class ComprovanteArmazenamentoServiceTests : IDisposable
{
    private readonly string _pastaTemporaria;
    private readonly ComprovanteArmazenamentoService _servico;

    public ComprovanteArmazenamentoServiceTests()
    {
        _pastaTemporaria = Directory.CreateTempSubdirectory("salgafacil-comprovantes-").FullName;
        var env = new FakeWebHostEnvironment { ContentRootPath = _pastaTemporaria };
        _servico = new ComprovanteArmazenamentoService(env, NullLogger<ComprovanteArmazenamentoService>.Instance);
    }

    public void Dispose()
    {
        try { Directory.Delete(_pastaTemporaria, recursive: true); }
        catch { /* best-effort — não deve derrubar a suíte de testes */ }
    }

    [Theory]
    [InlineData("comprovante.jpg")]
    [InlineData("comprovante.jpeg")]
    [InlineData("comprovante.PNG")] // extensão em maiúscula deve funcionar (comparação é OrdinalIgnoreCase)
    public void Validar_ImagemComAssinaturaCorreta_NaoLanca(string nomeArquivo)
    {
        var bytes = nomeArquivo.EndsWith("png", StringComparison.OrdinalIgnoreCase)
            ? ArquivosDeTeste.PngValido()
            : ArquivosDeTeste.JpgValido();

        var ex = Record.Exception(() => _servico.Validar(bytes, nomeArquivo));

        Assert.Null(ex);
    }

    [Fact]
    public void Validar_PdfComAssinaturaCorreta_NaoLanca()
    {
        var ex = Record.Exception(() => _servico.Validar(ArquivosDeTeste.PdfValido(), "comprovante.pdf"));

        Assert.Null(ex);
    }

    [Fact]
    public void Validar_ArquivoVazio_Lanca()
    {
        Assert.Throws<InvalidOperationException>(() => _servico.Validar([], "comprovante.jpg"));
    }

    [Fact]
    public void Validar_ArquivoMaiorQueLimite_Lanca()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _servico.Validar(ArquivosDeTeste.MaiorQueLimite(), "comprovante.jpg"));
    }

    [Fact]
    public void Validar_ExtensaoNaoSuportada_Lanca()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _servico.Validar(ArquivosDeTeste.PdfValido(), "comprovante.exe"));
    }

    [Fact]
    public void Validar_ConteudoNaoCorrespondeAAssinaturaDaExtensaoDeclarada_Lanca()
    {
        // Arquivo PDF renomeado para .jpg — a extensão diz "imagem", os bytes dizem "PDF".
        // Este é exatamente o cenário de "upload malicioso disfarçado" que o contrato pede
        // para bloquear, e é o motivo de validar por assinatura binária e não só por extensão.
        Assert.Throws<InvalidOperationException>(() =>
            _servico.Validar(ArquivosDeTeste.JpgFalsificado(), "comprovante.jpg"));
    }

    [Fact]
    public async Task SalvarAsync_ArquivoValido_GravaDentroDaPastaDaEmpresaERetornaChaveInterna()
    {
        var chave = await _servico.SalvarAsync(empresaId: 7, pedidoId: 42, ArquivosDeTeste.JpgValido(), "foto.jpg", caminhoAnterior: null);

        Assert.StartsWith("7/", chave);
        var lido = await _servico.LerAsync(chave);
        Assert.Equal(ArquivosDeTeste.JpgValido(), lido);

        var caminhoFisico = Path.Combine(_pastaTemporaria, "App_Data", "comprovantes", "7", chave["7/".Length..]);
        Assert.True(File.Exists(caminhoFisico));
    }

    [Fact]
    public async Task SalvarAsync_Reenvio_RemoveArquivoAnteriorDoMesmoPedido()
    {
        var chaveAntiga = await _servico.SalvarAsync(7, 42, ArquivosDeTeste.JpgValido(), "primeiro.jpg", null);
        var chaveNova = await _servico.SalvarAsync(7, 42, ArquivosDeTeste.PngValido(), "segundo.png", chaveAntiga);

        Assert.NotEqual(chaveAntiga, chaveNova);
        Assert.Null(await _servico.LerAsync(chaveAntiga));
        Assert.NotNull(await _servico.LerAsync(chaveNova));
    }

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("7/../../../segredo.txt")]
    [InlineData("../7/outro-arquivo.jpg")]
    public async Task LerAsync_ChaveTentandoEscaparDaPastaBase_RetornaNullSemLancar(string chaveMaliciosa)
    {
        var resultado = await _servico.LerAsync(chaveMaliciosa);

        Assert.Null(resultado);
    }

    [Theory]
    [InlineData("comprovante.jpg", "image/jpeg")]
    [InlineData("comprovante.jpeg", "image/jpeg")]
    [InlineData("comprovante.png", "image/png")]
    [InlineData("comprovante.pdf", "application/pdf")]
    [InlineData("comprovante.exe", "application/octet-stream")]
    public void ContentTypePara_RetornaTipoCorretoPorExtensao(string nomeArquivo, string esperado)
    {
        Assert.Equal(esperado, ComprovanteArmazenamentoService.ContentTypePara(nomeArquivo));
    }
}
