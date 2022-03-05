using System.IO;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace JoyMoe.Common.Storage;

public static class Hash
{
    public static byte[] Md5(this string cipher)
    {
        var bytes = Encoding.UTF8.GetBytes(cipher);
        return bytes.Md5();
    }

    public static byte[] Md5(this byte[] bytes)
    {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
        using var md5 = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
        return md5.ComputeHash(bytes);
    }

    public static byte[] Md5(this Stream stream)
    {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
        using var md5 = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
        return md5.ComputeHash(stream);
    }

    public static byte[] Sha1(this string cipher)
    {
        var bytes = Encoding.UTF8.GetBytes(cipher);
        return bytes.Sha1();
    }

    public static byte[] Sha1(this byte[] bytes)
    {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
        using var sha1 = SHA1.Create();
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
        return sha1.ComputeHash(bytes);
    }

    public static byte[] Sha1(this Stream stream)
    {
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
        using var sha1 = SHA1.Create();
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
        return sha1.ComputeHash(stream);
    }

    public static byte[] Sha256(this string cipher)
    {
        var bytes = Encoding.UTF8.GetBytes(cipher);
        return bytes.Sha256();
    }

    public static byte[] Sha256(this byte[] bytes)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(bytes);
    }

    public static byte[] Sha256(this Stream stream)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(stream);
    }
}
