namespace Vdcrpt.Next;

/// <summary>
/// An EffectContext 
/// </summary>
public class EffectContext
{
    public string CacheDir { get; init; }
    public string ScratchDir { get; init; }

    // cached files are local to input file and effect type
    // each input file gets its own cache, and within that each effect type gets its own cache
    // note that they are not local to instances - so differentiating data should be kept in the key
    public (string path, bool exists) GetCachedFile(string key, string extension = "")
    {
        var path = Path.ChangeExtension(Path.Combine(CacheDir, key), extension);

        if (File.Exists(key))
        {
            return (path, true);
        }

        return (path, false);
    }

    // returns a file that will be destroyed at the end of this session
    public string GetScratchFile(string? key = null, string extension = "")
    {
        key ??= Guid.NewGuid().ToString();
        return Path.ChangeExtension(Path.Combine(ScratchDir, key), extension);
    }
}
