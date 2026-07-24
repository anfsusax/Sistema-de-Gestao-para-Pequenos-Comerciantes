using System.Text.RegularExpressions;

namespace SalgaFacil.Domain.Services;

/// <summary>
/// Ponto único de normalização de telefone do sistema. Usado tanto para persistir
/// <see cref="Entities.Cliente.TelefoneNormalizado"/> quanto para consultar um cliente
/// existente antes de criar um novo (evita duplicidade — ver
/// _ia/DECISOES.md 2026-07-24 "Deduplicação de Cliente Por Telefone").
///
/// Regra assumida (mercado brasileiro, mesma suposição já usada em outras partes do
/// projeto — ex.: Estado fixo "SP" no cadastro via cardápio): sempre normaliza para
/// dígitos puros com DDI 55 na frente. Não valida se o número existe de fato, apenas
/// o formato — validação de "número real" (ex.: DDD válido) fica fora de escopo.
/// </summary>
public static class TelefoneNormalizador
{
    private static readonly Regex NaoDigito = new(@"\D+", RegexOptions.Compiled);

    /// <summary>
    /// Normaliza um telefone para dígitos puros com DDI 55.
    /// Retorna null se o valor de entrada não tiver dígitos suficientes para ser um telefone válido
    /// (menos de 10 dígitos após remover DDI, ou seja, nem DDD+fixo cabe).
    /// </summary>
    public static string? Normalizar(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return null;

        var digitos = NaoDigito.Replace(telefone, "");
        if (digitos.Length == 0)
            return null;

        // Remove zero(s) de discagem internacional/tronco eventualmente digitados (ex.: "0055...", "0 11 9...").
        digitos = digitos.TrimStart('0');

        // Já tem DDI 55 (13 dígitos = 55 + DDD + 9 dígitos; 12 dígitos = 55 + DDD + 8 dígitos fixo).
        if (digitos.Length is 12 or 13 && digitos.StartsWith("55"))
            return digitos;

        // DDD + numero, sem DDI (11 dígitos = celular com 9; 10 dígitos = fixo).
        if (digitos.Length is 10 or 11)
            return "55" + digitos;

        // Formato não reconhecido (curto demais, ou DDI diferente de 55 — fora do escopo assumido).
        // Preserva os dígitos como estão em vez de descartar silenciosamente: dois valores igualmente
        // "estranhos" continuam comparáveis entre si, mas não tentamos adivinhar o DDI.
        return digitos.Length >= 8 ? digitos : null;
    }

    /// <summary>Atalho para validação: true se <paramref name="telefone"/> normaliza para um valor utilizável.</summary>
    public static bool EhValido(string? telefone) => Normalizar(telefone) is not null;
}
