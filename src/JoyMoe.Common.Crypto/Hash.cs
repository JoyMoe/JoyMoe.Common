using System.IO;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class Hash
    {
        public static byte[] Md5(this string cipher)
        {
            return Encoding.UTF8.GetBytes(cipher).Md5();
        }

        public static byte[] Md5(this byte[] cipher)
        {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
            using var md5 = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            return md5.ComputeHash(cipher);
        }

        public static byte[] Md5(this Stream cipher)
        {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
            using var md5 = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            return md5.ComputeHash(cipher);
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

        public static byte[] Sha1(this Stream cipher)
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

        public static byte[] Sha256(this Stream cipher)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(cipher);
        }
    }
}
