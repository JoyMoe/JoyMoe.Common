// ReSharper disable once CheckNamespace
namespace System
{
    public static class Helper
    {
        public static string ToHex(this byte[] bytes)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return BitConverter.ToString(bytes)
                .ToLowerInvariant()
                .Replace("-", string.Empty);
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        public static byte[] ToBytes(string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex));
            }

            var length = hex.Length;
            var bytes = new byte[length / 2];
            for (var i = 0; i < length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
