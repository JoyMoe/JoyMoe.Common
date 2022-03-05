using System.Text.Json;

namespace JoyMoe.Common.Json;

/// <summary>
/// LOWER CASE PROPERTY NAMING POLICY
/// </summary>
public class UpperCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly UpperCaseNamingPolicy Instance = new();

    /// <inheritdoc/>
    public override string ConvertName(string name) {
        return string.IsNullOrEmpty(name) ? name : name.ToUpperInvariant();
    }
}
