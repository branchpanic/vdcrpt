using FFMpegCore;
using FFMpegCore.Arguments;

namespace Vdcrpt;

/// <summary>
/// A Session is the environment used to apply IEffects to a video.
/// </summary>
public class Session : IDisposable
{
    private readonly Guid _sessionId;

    // TODO: Allow changing the input file after effects have already been applied.
    private readonly string _initialFile;

    private readonly string _scratchRoot; // Deleted when session is disposed.
    private readonly string _cacheRoot; // Kept between sessions.

    private readonly List<IEffect> _effects;

    public Session(string initialFile, Guid? sessionId = null, string? tempDir = null)
    {
        _sessionId = sessionId ?? Guid.NewGuid();

        tempDir ??= Path.Join(Path.GetTempPath(), "vdcrpt");
        _effects = new List<IEffect>();
        _initialFile = initialFile;

        _cacheRoot = Path.Join(tempDir, "cache");

        var inputKey = CacheUtility.GetKeyFromContents(initialFile);
        _scratchRoot = Path.Join(tempDir, $"scratch-{inputKey}-{_sessionId}");

        Directory.CreateDirectory(_cacheRoot);
        Directory.CreateDirectory(_scratchRoot);
    }

    public Session Add(IEffect effect)
    {
        _effects.Add(effect);
        return this;
    }

    private EffectContext MakeContext(string effectKey)
    {
        return new EffectContext(
            cacheDir: Path.Join(_cacheRoot, effectKey),
            scratchDir: Path.Join(_scratchRoot, effectKey + "-" + Guid.NewGuid())
        );
    }

    private FFMpegArgumentProcessor BuildFinalOutputArgs(
        string internalOutput,
        string output,
        Action<FFMpegArgumentOptions> buildOutputArgs
    ) => FFMpegArguments
            .FromFileInput(internalOutput)
            .WithGlobalOptions(args => args.WithVerbosityLevel(VerbosityLevel.Fatal))
            .OutputToFile(output, true, buildOutputArgs);

    public void Render(string output) =>
        Render(output, args =>
            args.WithoutMetadata()
                .WithCustomArgument("-fflags +genpts")  // Fix unwated timecode issues
                .WithVideoCodec("libx264")
                .WithAudioCodec("aac")
        );

    public void Render(string output, Action<FFMpegArgumentOptions> buildOutputArgs)
    {
        var result = ApplyEffects();
        BuildFinalOutputArgs(result, output, buildOutputArgs).ProcessSynchronously();
    }

    public async Task RenderAsync(string output) =>
        await RenderAsync(output, args =>
            args.WithoutMetadata()
                .WithCustomArgument("-fflags +genpts")  // Fix unwated timecode issues
                .WithVideoCodec("libx264")
                .WithAudioCodec("aac")
        );

    public async Task RenderAsync(string output, Action<FFMpegArgumentOptions> buildOutputArgs)
    {
        var result = ApplyEffects();
        await BuildFinalOutputArgs(result, output, buildOutputArgs).ProcessAsynchronously();
    }

    private string ApplyEffects()
    {
        var prev = _initialFile;
        var current = prev;

        for (var i = 0; i < _effects.Count; i++)
        {
            var effect = _effects[i];
            current = Path.Join(_cacheRoot, $"render-{i:0000}");
            effect.Apply(MakeContext(effect.GetType().ToString()), prev, current);

            if (!File.Exists(current))
            {
                throw new InvalidOperationException($"Effect {effect} ({effect.GetType()}) did not produce output.");
            }

            prev = current;
        }

        return current;
    }

    public void Dispose()
    {
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
            session.Add(effect);
        }

        session.Render(outputPath);
    }
}