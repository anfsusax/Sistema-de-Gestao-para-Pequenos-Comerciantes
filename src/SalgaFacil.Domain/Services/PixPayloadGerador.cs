using System.Globalization;
using System.Text;

namespace SalgaFacil.Domain.Services;

/// <summary>
/// Gera o payload Pix estático no formato BR Code / EMV (especificação do Banco Central —
/// "Manual de Padrões para Iniciação do Pix"), incluindo o CRC16 final. Não depende de gateway,
/// banco ou serviço externo: é só formatação de texto (TLV — Tag/Length/Value) + um checksum.
///
/// PIX-MANUAL-002. Usado por PagamentoPixService para montar o "Pix Copia e Cola" exibido ao
/// cliente e como entrada do gerador de QR Code (QrCodeGerador, camada Web — este projeto
/// Domain não depende de bibliotecas de imagem).
/// </summary>
public static class PixPayloadGerador
{
    private const string GuiPix = "br.gov.bcb.pix";
    private const int TamanhoMaximoNomeBeneficiario = 25;
    private const int TamanhoMaximoCidade = 15;
    private const int TamanhoMaximoTxid = 25;
    private const int TamanhoMaximoDescricao = 40;

    public sealed record Dados(
        string ChavePix,
        string NomeBeneficiario,
        string? Cidade,
        decimal Valor,
        string IdentificadorTransacao,
        string? Descricao = null);

    /// <summary>
    /// Monta o payload completo, já com o campo 63 (CRC16) calculado e anexado ao final.
    /// Lança <see cref="ArgumentException"/> se os dados obrigatórios não permitirem montar um
    /// Pix válido (chave, beneficiário ou valor ausentes/inválidos) — validação de negócio
    /// (empresa com Pix inativo, etc.) é responsabilidade de quem chama, não deste gerador.
    /// </summary>
    public static string GerarPayload(Dados dados)
    {
        if (string.IsNullOrWhiteSpace(dados.ChavePix))
            throw new ArgumentException("Chave Pix é obrigatória para gerar o payload.", nameof(dados));
        if (string.IsNullOrWhiteSpace(dados.NomeBeneficiario))
            throw new ArgumentException("Nome do beneficiário é obrigatório para gerar o payload.", nameof(dados));
        if (dados.Valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero para gerar o payload.", nameof(dados));

        var nome = Sanitizar(dados.NomeBeneficiario, TamanhoMaximoNomeBeneficiario, "LOJA");
        var cidade = Sanitizar(string.IsNullOrWhiteSpace(dados.Cidade) ? "BRASIL" : dados.Cidade, TamanhoMaximoCidade, "BRASIL");
        var txid = SanitizarTxid(dados.IdentificadorTransacao);
        var valor = dados.Valor.ToString("F2", CultureInfo.InvariantCulture);

        var infoContaComerciante = Campo("00", GuiPix) + Campo("01", dados.ChavePix.Trim());
        if (!string.IsNullOrWhiteSpace(dados.Descricao))
            infoContaComerciante += Campo("02", Sanitizar(dados.Descricao, TamanhoMaximoDescricao, ""));

        var dadosAdicionais = Campo("05", txid);

        var semCrc =
            Campo("00", "01") +               // Payload Format Indicator
            Campo("01", "12") +                // Point of Initiation Method: 12 = valor fixo, uso único
            Campo("26", infoContaComerciante) + // Merchant Account Information (Pix)
            Campo("52", "0000") +               // Merchant Category Code (não especificado)
            Campo("53", "986") +                // Transaction Currency: 986 = BRL (ISO 4217)
            Campo("54", valor) +                // Transaction Amount
            Campo("58", "BR") +                 // Country Code
            Campo("59", nome) +                 // Merchant Name
            Campo("60", cidade) +               // Merchant City
            Campo("62", dadosAdicionais) +      // Additional Data Field Template (txid)
            "6304";                             // ID+Length do CRC16 — o valor vem logo abaixo

        return semCrc + CalcularCrc16(semCrc);
    }

    private static string Campo(string id, string valor) => $"{id}{valor.Length:D2}{valor}";

    /// <summary>
    /// CRC-16/CCITT-FALSE (poly 0x1021, init 0xFFFF, sem reflexão, xorout 0x0000) — a variante
    /// exigida pela especificação do BR Code. Retorna 4 dígitos hexadecimais maiúsculos.
    /// Vetor de teste padrão da família CRC-16/CCITT-FALSE: CalcularCrc16("123456789") == "29B1"
    /// (conferido em PixPayloadGeradorTests — é o "check value" catalogado para este algoritmo,
    /// independente do domínio Pix, então serve como prova objetiva de que a implementação está
    /// correta antes mesmo de testar payloads Pix completos).
    /// </summary>
    public static string CalcularCrc16(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        ushort crc = 0xFFFF;

        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 0x8000) != 0
                    ? (ushort)((crc << 1) ^ 0x1021)
                    : (ushort)(crc << 1);
            }
        }

        return crc.ToString("X4");
    }

    /// <summary>
    /// Remove acentuação e caracteres fora de A-Z/0-9/espaço (o BR Code recomenda ASCII simples
    /// nos campos de texto para máxima compatibilidade entre apps bancários), maiúsculas, e
    /// trunca no tamanho máximo do campo. Nunca retorna string vazia — cai no <paramref name="valorPadrao"/>.
    /// </summary>
    private static string Sanitizar(string valor, int tamanhoMaximo, string valorPadrao)
    {
        var semAcento = RemoverAcentos(valor.Trim().ToUpperInvariant());
        var limpo = new string(semAcento.Where(c => char.IsAsciiLetterOrDigit(c) || c == ' ').ToArray()).Trim();
        if (string.IsNullOrWhiteSpace(limpo))
            limpo = valorPadrao;

        return limpo.Length > tamanhoMaximo ? limpo[..tamanhoMaximo].TrimEnd() : limpo;
    }

    /// <summary>Txid: só alfanumérico (sem espaço), conforme especificação. "***" é o coringa documentado para "sem txid".</summary>
    private static string SanitizarTxid(string valor)
    {
        var limpo = new string(valor.Where(char.IsAsciiLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(limpo))
            limpo = "***";

        return limpo.Length > TamanhoMaximoTxid ? limpo[..TamanhoMaximoTxid] : limpo;
    }

    private static string RemoverAcentos(string texto)
    {
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
