using System.Security.Cryptography;
using FFMpegCore;
using FFMpegCore.Arguments;

namespace Vdcrpt.Next;

/// <summary>
/// A Session is the main entry point for applying effects to videos.
/// </summary>
public class Session : IDisposable
{
    private readonly string _inputKey;
    private readonly string _initialFile;

    private readonly string _scratchRoot; // Deleted when session is disposed.
    private readonly string _cacheRoot; // Kept between sessions.

    private readonly List<IEffect> _effects;

    public Session(string initialFile, string? tempDir = null)
    {
        tempDir ??= Path.Join(Path.GetTempPath(), "vdcrpt");
        _effects = new List<IEffect>();
        _initialFile = initialFile;

        using (var stream = File.OpenRead(initialFile))
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                _inputKey = BitConverter.ToString(hash);
            }
        }

        _cacheRoot = Path.Join(tempDir, _inputKey, "cache");
        _scratchRoot = Path.Join(tempDir, _inputKey, "scratch-" + Guid.NewGuid());

        Directory.CreateDirectory(_cacheRoot);
        Directory.CreateDirectory(_scratchRoot);
    }

    public Session Apply(IEffect effect)
    {
        _effects.Add(effect);
        return this;
    }

    private EffectContext CreateContext(string effectKey)
    {
        return new EffectContext
        {
            CacheDir = Path.Join(_cacheRoot, effectKey),
            ScratchDir = Path.Join(_scratchRoot, effectKey + "-" + Guid.NewGuid())
        };
    }

    public void Render(string output) =>
        Render(output, args =>
            args.WithoutMetadata()
                .WithCustomArgument("-fflags +genpts")
                .WithVideoCodec("libx264")
                .WithAudioCodec("aac")
        );
    
    public void Render(string output, Action<FFMpegArgumentOptions> buildOutputArgs)
    {
        var prev = _initialFile;
        var current = prev;

        for (var i = 0; i < _effects.Count; i++)
        {
            var effect = _effects[i];
            current = Path.Join(_cacheRoot, $"render-{i:0000}");
            effect.Apply(CreateContext(effect.GetType().ToString()), prev, current);
            prev = current;
        }

        FFMpegArguments
            .FromFileInput(current)
            .WithGlobalOptions(args => args.WithVerbosityLevel(VerbosityLevel.Fatal))
            .OutputToFile(output, true, buildOutputArgs)
            .ProcessSynchronously();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Directory.Delete(_scratchRoot, true);
    }

    /// <summary>
    /// Applies the given effects to the input file, then saves the result to the output file.
    /// 
    /// All the usual Session caching still applies. This method just wraps creating and disposing the session for
    /// accelerated "one-shot" use.
    /// </summary>
    public static void ApplyEffects(string inputPath, string outputPath, params IEffect[] effects)
    {
        using var session = new Session(inputPath);
        
        foreach (var effect in effects)
        {
            session.Apply(effect);
        }
        
        session.Render(outputPath);
    }
}