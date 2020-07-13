using System;
using System.IO;
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

        public static byte[] Md5(this Stream cipher)
        {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
            using var sha1 = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            return sha1.ComputeHash(cipher);
        }

        public static byte[] Sha1(this string cipher)
        {
            return Encoding.UTF8.GetBytes(cipher).Sha1();
        }

        public static byte[] Sha1(this byte[] cipher)
        {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
            using var sha1 = SHA1.Create();
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
            return sha1.ComputeHash(cipher);
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

        public static byte[] HmacSha1(this string cipher, string key)
        {
            return cipher.HmacSha1(Encoding.UTF8.GetBytes(key));
        }

        public static byte[] HmacSha1(this string cipher, byte[] key)
        {
            return Encoding.UTF8.GetBytes(cipher).HmacSha1(key);
        }

        public static byte[] HmacSha1(this byte[] cipher, string key)
        {
            return cipher.HmacSha1(Encoding.UTF8.GetBytes(key));
        }

        public static byte[] HmacSha1(this byte[] cipher, byte[] key)
        {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
            using var hmac = new HMACSHA1(key);
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
            return hmac.ComputeHash(cipher);
        }

        public static byte[] HmacSha256(this string cipher, string key)
        {
            return cipher.HmacSha256(Encoding.UTF8.GetBytes(key));
        }

        public static byte[] HmacSha256(this string cipher, byte[] key)
        {
            return Encoding.UTF8.GetBytes(cipher).HmacSha256(key);
        }

        public static byte[] HmacSha256(this byte[] cipher, string key)
        {
            return cipher.HmacSha256(Encoding.UTF8.GetBytes(key));
        }

        public static byte[] HmacSha256(this byte[] cipher, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(cipher);
        }
    }
}
