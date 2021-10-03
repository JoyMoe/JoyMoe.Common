using System.Text.Json;

namespace JoyMoe.Common.Json;

/// <summary>
/// lower case property naming policy
/// </summary>
public class LowerCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly LowerCaseNamingPolicy Instance = new();

    /// <inheritdoc/>
    public override string ConvertName(string name)
    {
#pragma warning disable CA1308 // Normalize strings to uppercase
        return string.IsNullOrEmpty(name) ? name : name.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
    }
}
