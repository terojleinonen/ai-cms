using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cms.Application.Common;

public static partial class Slug
{
    public const int MaxLength = 120;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex ValidPattern();

    public static bool IsValid(string? value) =>
        !string.IsNullOrEmpty(value) && value.Length <= MaxLength && ValidPattern().IsMatch(value);

    /// <summary>Converts arbitrary text to a lowercase, hyphenated, ASCII URL slug.</summary>
    public static string From(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        var lastHyphen = true;
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            var lower = char.ToLowerInvariant(c);
            if (lower is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                sb.Append(lower);
                lastHyphen = false;
            }
            else if (!lastHyphen)
            {
                sb.Append('-');
                lastHyphen = true;
            }
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > MaxLength) slug = slug[..MaxLength].TrimEnd('-');
        return slug;
    }
}
