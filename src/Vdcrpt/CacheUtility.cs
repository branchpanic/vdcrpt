using System.Security.Cryptography;

namespace Vdcrpt;

public static class CacheUtility
{
    public static string GetKeyFromContents(string path)
    {
        using var stream = File.OpenRead(path);
        return GetKeyFromContents(stream);
    }

    public static string GetKeyFromContents(Stream sr)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(sr);
        return BitConverter.ToString(hash);
    }

    public static string SanitizeFileName(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
    }
}
