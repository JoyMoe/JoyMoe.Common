using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

internal static class CaseConverter
{
    public static string ToSnakeCase(this string text) {
        if (text.Length < 2) return text;

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));
        for (var i = 1; i < text.Length; ++i) {
            var c = text[i];
            if (char.IsUpper(c)) {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            } else {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
