namespace Vdcrpt.Next;

/// <summary>
/// An IEffect takes a video file and produces another video file.
///
/// IEffects are heavier than traditional video filters. In vdcrpt, they operate on encoded video data, intentionally
/// trying to create interesting artifacts without rendering the result unplayable.
///
/// When working with files, IEffects should use the "scratch" and "cache" mechanisms provided by their given
/// EffectContexts. However, they shouldn't memoize themselves entirely -- that should be left up to the caller.
/// </summary>
public interface IEffect
{
    void Apply(EffectContext context, string inputPath, string outputPath)
    {
        File.Copy(inputPath, outputPath);
    }
}
