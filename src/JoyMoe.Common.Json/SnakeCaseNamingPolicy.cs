using System.Globalization;
using System.Text;
using System.Text.Json;

namespace JoyMoe.Common.Json;

/// <summary>
/// snake_case_json_naming_policy
/// </summary>
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly SnakeCaseNamingPolicy Instance = new();

    /// <inheritdoc/>
    public override string ConvertName(string name) {
        // Port from https://github.com/efcore/EFCore.NamingConventions/blob/7f07dcce613ba5f67c92ec4d3357c14b461db79e/EFCore.NamingConventions/Internal/SnakeCaseNameRewriter.cs

        if (string.IsNullOrEmpty(name)) {
            return name;
        }

        var builder          = new StringBuilder(name.Length + Math.Min(2, name.Length / 5));
        var previousCategory = default(UnicodeCategory?);

        for (var currentIndex = 0; currentIndex < name.Length; currentIndex++) {
            var currentChar = name[currentIndex];
            if (currentChar == '_') {
                builder.Append('_');
                previousCategory = null;
                continue;
            }

            var currentCategory = char.GetUnicodeCategory(currentChar);
            switch (currentCategory) {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == UnicodeCategory.SpaceSeparator ||
                        previousCategory == UnicodeCategory.LowercaseLetter ||
                        (previousCategory != null &&
                         currentIndex > 0 &&
                         currentIndex + 1 < name.Length &&
                         char.IsLower(name[currentIndex + 1]))) {
                        builder.Append('_');
                    }

                    currentChar = char.ToLowerInvariant(currentChar);
                    break;

                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == UnicodeCategory.SpaceSeparator) {
                        builder.Append('_');
                    }

                    break;

                default:
                    if (previousCategory != null) {
                        previousCategory = UnicodeCategory.SpaceSeparator;
                    }

                    continue;
            }

            builder.Append(currentChar);
            previousCategory = currentCategory;
        }

        return builder.ToString();
    }
}
