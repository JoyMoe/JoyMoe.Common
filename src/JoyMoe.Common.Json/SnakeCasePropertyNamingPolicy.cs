using System.Linq;
using System.Text.Json;

namespace JoyMoe.Common.Json
{
    /// <summary>
    /// snake_case_json_naming_policy
    /// </summary>
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        /// <inheritdoc/>
        public override string ConvertName(string name)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return string.Concat(name.Select((character, index) =>
                    index > 0 && char.IsUpper(character)
                        ? "_" + character
                        : character.ToString()))
                .ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        }
    }
}
