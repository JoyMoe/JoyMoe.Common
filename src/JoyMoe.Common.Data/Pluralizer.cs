using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System;

public static class Pluralizer
{
    public static string Pluralize(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var exceptions = new Dictionary<string, string>()
        {
            { "man", "men" },
            { "woman", "women" },
            { "child", "children" },
            { "tooth", "teeth" },
            { "foot", "feet" },
            { "mouse", "mice" },
            { "belief", "beliefs" }
        };

#pragma warning disable CA1308 // Normalize strings to uppercase
        if (exceptions.ContainsKey(text.ToLowerInvariant()))
        {
            return exceptions[text.ToLowerInvariant()];
        }
#pragma warning restore CA1308 // Normalize strings to uppercase

        if (text.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
            !text.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
            !text.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
            !text.EndsWith("iy", StringComparison.OrdinalIgnoreCase) &&
            !text.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
            !text.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
        {
            return text.Substring(0, text.Length - 1) + "ies";
        }

        if (text.EndsWith("us", StringComparison.OrdinalIgnoreCase))
        {
            // http://en.wikipedia.org/wiki/Plural_form_of_words_ending_in_-us
            return text + "es";
        }

        if (text.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
        {
            return text + "es";
        }

        if (text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        if (text.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            text.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            text.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return text + "es";
        }

        if (text.EndsWith("f", StringComparison.OrdinalIgnoreCase) && text.Length > 1)
        {
            return text.Substring(0, text.Length - 1) + "ves";
        }

        if (text.EndsWith("fe", StringComparison.OrdinalIgnoreCase) && text.Length > 2)
        {
            return text.Substring(0, text.Length - 2) + "ves";
        }

        return text + "s";
    }
}
