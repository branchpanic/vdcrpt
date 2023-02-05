using FFMpegCore;

namespace Vdcrpt.Next;

/// <summary>
/// An EffectContext describes the working environment for an effect.
///
/// Effects should use the provided methods for getting "scratch" (temporary, disposable) and "cache" (reusable) file
/// paths. The former allows for easier cleanup, while the latter cuts down on repeated work.
///
/// Some common operations, like converting audio and/or video codecs using FFMpeg, are provided as extension methods
/// with automatic caching.
/// </summary>
public class EffectContext
{
    public string CacheDir { get; }
    public string ScratchDir { get; }

    public EffectContext(string cacheDir, string scratchDir)
    {
        CacheDir = cacheDir;
        ScratchDir = scratchDir;
    }

    /// <summary>
    /// Gets a path to a unique temporary file that is deleted when the session ends.
    /// </summary>
    public string GetScratchFile(string extension = "")
    {
        var key = Guid.NewGuid().ToString();
        return Path.ChangeExtension(Path.Combine(ScratchDir, key), extension);
    }

    /// <summary>
    /// Gets a cached file using an arbitrary key, returning a boolean indicating whether the file already exists.
    /// Cached files are local to effect types, and persist between runs.
    /// </summary>
    public (string path, bool exists) GetCachedFile(string key, string extension = "")
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cache key must not be null or empty", nameof(key));
        }

        return GetCachedFileInternal(CacheUtility.SanitizeFileName(key), extension);
    }

    /// <summary>
    /// Gets a cached file using an arbitrary compound key (where order of components matters), returning a boolean
    /// indicating whether the file already exists. Cached files are local to effect types, and persist between runs.
    /// </summary>
    public (string path, bool exists) GetCachedFile(string[] keys, string extension = "")
    {
        if (keys == null)
        {
            throw new ArgumentNullException(nameof(keys));
        }

        var compoundKey = Path.Combine(keys.Select(k =>
        {
            if (string.IsNullOrEmpty(k))
            {
                throw new ArgumentException("Cache key component must not be null or empty");
            }

            return CacheUtility.SanitizeFileName(k);
        }).ToArray());

        return GetCachedFileInternal(compoundKey, extension);
    }

    private (string path, bool exists) GetCachedFileInternal(string filenameSafeKey, string extension)
    {
        var path = Path.ChangeExtension(Path.Join(CacheDir, filenameSafeKey), extension);

        if (!File.Exists(path))
        {
            if (Directory.GetParent(path) is { } parent)
            {
                Directory.CreateDirectory(parent.FullName);
            }

            return (path, false);
        }

        return (path, true);
    }
}

public static class EffectContextExtensions
{
    /// <summary>
    /// Gets a cached file whose key is based on the contents of another file.
    /// 
    /// This is especially useful for preprocessing steps that have a deterministic output based on an input file.
    /// </summary>
    public static (string path, bool exists) GetDerivedFile(
        this EffectContext context,
        string originalFile,
        string derivedFileKey,
        string extension = ""
    )
    {
        return context.GetCachedFile(
            new[] { CacheUtility.GetKeyFromContents(originalFile), derivedFileKey },
            extension
        );
    }

    /// <summary>
    /// Converts a given file using FFMpeg and caches the result.
    /// </summary>
    public static string ConvertFile(
        this EffectContext context,
        string inputPath,
        string vcodec,
        string acodec,
        string format
    )
    {
        var (path, exists) = context.GetDerivedFile(inputPath, $"v{vcodec}_a{acodec}", format);
        if (exists)
        {
            return path;
        }

        FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(path, overwrite: true, args =>
                args.WithoutMetadata()
                    .WithVideoCodec(vcodec)
                    .WithAudioCodec(acodec)
                    .ForceFormat(format))
            .ProcessSynchronously();

        return path;
    }
}
