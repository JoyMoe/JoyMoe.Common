using System;

// ReSharper disable once CheckNamespace
namespace JoyMoe.Common.Storage;

public static class Helper
{
    public static string ToHex(this byte[] bytes) {
#pragma warning disable CA1308 // Normalize strings to uppercase
        return BitConverter.ToString(bytes).ToLowerInvariant().Replace("-", string.Empty);
#pragma warning restore CA1308 // Normalize strings to uppercase
    }
}
