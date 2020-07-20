using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class Hmac
    {
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
