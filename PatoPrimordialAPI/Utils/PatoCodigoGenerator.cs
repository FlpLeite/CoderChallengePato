using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PatoPrimordialAPI.Utils;

public static class PatoCodigoGenerator
{
    public static string GerarCodigo(string pais, string cidade, double latitude, double longitude)
    {
        var lat = Math.Round(latitude, 4, MidpointRounding.AwayFromZero);
        var lon = Math.Round(longitude, 4, MidpointRounding.AwayFromZero);
        var paisSlug = Slug(pais);
        var cidadeSlug = Slug(cidade);
        return $"{paisSlug}-{cidadeSlug}-{lat.ToString("0.0000", CultureInfo.InvariantCulture)}-{lon.ToString("0.0000", CultureInfo.InvariantCulture)}";
    }

    private static string Slug(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return "desconhecido";
        }

        var normalized = texto.ToLowerInvariant();
        normalized = normalized.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (char.IsWhiteSpace(c) || c == '-' || c == '_')
            {
                sb.Append('-');
            }
        }

        var slug = Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "desconhecido" : slug;
    }
}
