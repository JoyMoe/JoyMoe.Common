using System;
using System.Security.Cryptography;
using System.Text;

namespace JoyMoe.Common.Storage
{
    public static class CryptoHelper
    {
        public static string ToHex(this byte[] bytes)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return BitConverter.ToString(bytes)
                .ToLowerInvariant()
                .Replace("-", string.Empty);
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        public static byte[] Sha256(this string cipher)
        {
            return Encoding.UTF8.GetBytes(cipher).Sha256();
        }

        public static byte[] Sha256(this byte[] cipher)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(cipher);
        }

        public static byte[] HmacSha256(this string cipher, string key)
        {
            return cipher.HmacSha256(Encoding.UTF8.GetBytes(key));
        }

        public static byte[] HmacSha256(this string cipher, byte[] key)
        {
            return Encoding.UTF8.GetBytes(cipher).HmacSha256(key);
        }

        public static byte[] HmacSha256(this byte[] cipher, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(cipher);
        }
    }
}
