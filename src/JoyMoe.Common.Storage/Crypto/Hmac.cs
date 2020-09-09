using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace JoyMoe.Common.Storage
{
    public static class Hmac
    {
        public static byte[] HmacSha1(this string cipher, string key)
        {
            var keys = Encoding.UTF8.GetBytes(key);
            return cipher.HmacSha1(keys);
        }

        public static byte[] HmacSha1(this string cipher, byte[] keys)
        {
            var bytes = Encoding.UTF8.GetBytes(cipher);
            return bytes.HmacSha1(keys);
        }

        public static byte[] HmacSha1(this byte[] bytes, byte[] keys)
        {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
            using var hmac = new HMACSHA1(keys);
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
            return hmac.ComputeHash(bytes);
        }

        public static byte[] HmacSha256(this string cipher, string key)
        {
            var keys = Encoding.UTF8.GetBytes(key);
            return cipher.HmacSha256(keys);
        }

        public static byte[] HmacSha256(this string cipher, byte[] keys)
        {
            var bytes = Encoding.UTF8.GetBytes(cipher);
            return bytes.HmacSha256(keys);
        }

        public static byte[] HmacSha256(this byte[] bytes, byte[] keys)
        {
            using var hmac = new HMACSHA256(keys);
            return hmac.ComputeHash(bytes);
        }
    }
}
