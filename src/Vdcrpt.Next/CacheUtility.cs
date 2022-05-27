using System.Security.Cryptography;

namespace Vdcrpt.Next;

public static class CacheUtility
{
    public static string GetKeyFromFile(string path)
    {
        // Could have an overload that also returns this stream to avoid repeated reads, but we'll do that when we need
        // it.
        using var stream = File.OpenRead(path);

        // Don't depend on the result of this function being an MD5 hash, it could eventually change.
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash);
    }

    public static string SanitizeFileName(string name)
    {
        return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
    }
}
