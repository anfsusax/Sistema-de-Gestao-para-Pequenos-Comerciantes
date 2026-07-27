using SalgaFacil.Domain.Services;
using Xunit;

namespace SalgaFacil.Tests.Domain;

public class PixPayloadGeradorTests
{
    [Fact]
    public void CalcularCrc16_VetorPadrao_RetornaValorCatalogadoDoAlgoritmo()
    {
        // "123456789" -> "29B1" é o check value oficial da família CRC-16/CCITT-FALSE
        // (poly 0x1021, init 0xFFFF) — usado pela especificação BR Code do Pix. Provar isto
        // isoladamente do domínio Pix garante que a implementação do algoritmo em si está
        // correta antes de testar payloads completos.
        Assert.Equal("29B1", PixPayloadGerador.CalcularCrc16("123456789"));
    }

    [Fact]
    public void GerarPayload_DadosValidos_CrcNoFinalConfereComORestanteDoPayload()
    {
        var dados = new PixPayloadGerador.Dados(
            ChavePix: "loja@salgadosfacil.com.br",
            NomeBeneficiario: "Salgados Facil",
            Cidade: "Sao Paulo",
            Valor: 42.50m,
            IdentificadorTransacao: "PED1234");

        var payload = PixPayloadGerador.GerarPayload(dados);

        Assert.True(payload.Length > 4);
        var semCrc = payload[..^4];
        var crcInformado = payload[^4..];
        Assert.Equal(PixPayloadGerador.CalcularCrc16(semCrc), crcInformado);
    }

    [Fact]
    public void GerarPayload_DadosValidos_ContemCamposObrigatoriosDoBrCode()
    {
        var dados = new PixPayloadGerador.Dados(
            ChavePix: "11999998888",
            NomeBeneficiario: "Salgados Facil",
            Cidade: "Sao Paulo",
            Valor: 10.00m,
            IdentificadorTransacao: "PED1");

        var payload = PixPayloadGerador.GerarPayload(dados);

        Assert.StartsWith("000201", payload); // Payload Format Indicator (00) + Point of Initiation Method (01)
        Assert.Contains("5303986", payload);   // Transaction Currency = 986 (BRL)
        Assert.Contains("5802BR", payload);    // Country Code = BR
        Assert.Contains("540510.00", payload); // Transaction Amount = 10.00 (campo 54, tamanho 5)
        Assert.Contains(dados.ChavePix, payload);
        Assert.Contains("6304", payload);      // ID+tamanho do campo CRC16
    }

    [Fact]
    public void GerarPayload_NomeComAcentoEMinusculas_NormalizaParaMaiusculaSemAcento()
    {
        var dados = new PixPayloadGerador.Dados(
            ChavePix: "chave-pix",
            NomeBeneficiario: "Café Padaria Ltda",
            Cidade: null,
            Valor: 5.00m,
            IdentificadorTransacao: "PED2");

        var payload = PixPayloadGerador.GerarPayload(dados);

        Assert.Contains("CAFE PADARIA LTDA", payload);
        Assert.DoesNotContain("é", payload);
        Assert.DoesNotContain("Café", payload);
    }

    [Fact]
    public void GerarPayload_IdentificadorComCaracteresInvalidos_MantemSoAlfanumerico()
    {
        var dados = new PixPayloadGerador.Dados(
            ChavePix: "chave-pix",
            NomeBeneficiario: "Loja",
            Cidade: null,
            Valor: 5.00m,
            IdentificadorTransacao: "PED#123!");

        var payload = PixPayloadGerador.GerarPayload(dados);

        Assert.Contains("PED123", payload);
        Assert.DoesNotContain("PED#123!", payload);
    }

    [Fact]
    public void GerarPayload_NomeBeneficiarioMuitoLongo_TruncaEm25Caracteres()
    {
        var dados = new PixPayloadGerador.Dados(
            ChavePix: "chave-pix",
            NomeBeneficiario: new string('A', 40),
            Cidade: null,
            Valor: 5.00m,
            IdentificadorTransacao: "PED3");

        var payload = PixPayloadGerador.GerarPayload(dados);

        Assert.Contains($"59{25:D2}{new string('A', 25)}", payload);
        Assert.DoesNotContain(new string('A', 26), payload);
    }

    [Theory]
    [InlineData("", "Nome", 1.0)]
    [InlineData("chave", "", 1.0)]
    [InlineData("chave", "Nome", 0)]
    [InlineData("chave", "Nome", -1.0)]
    public void GerarPayload_DadosObrigatoriosAusentesOuValorInvalido_LancaArgumentException(
        string chave, string nome, decimal valor)
    {
        var dados = new PixPayloadGerador.Dados(chave, nome, null, valor, "TXID");

        Assert.Throws<ArgumentException>(() => PixPayloadGerador.GerarPayload(dados));
    }
}
