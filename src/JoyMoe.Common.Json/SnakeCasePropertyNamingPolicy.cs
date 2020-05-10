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
            return string.Concat(name.Select((character, index) =>
                    index > 0 && char.IsUpper(character)
                        ? "_" + character
                        : character.ToString()))
                .ToLower();
        }
    }
}
