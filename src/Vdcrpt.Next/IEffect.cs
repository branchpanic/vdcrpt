namespace Vdcrpt.Next;

/// <summary>
/// An IEffect takes a video file and produces another video file.
///
/// IEffects are heavier than traditional video filters. In vdcrpt, they operate on encoded video data, intentionally
/// trying to create interesting artifacts without rendering the result unplayable.
///
/// When working with files, IEffects should use the "scratch" and "cache" mechanisms provided by their given
/// EffectContexts.
///
/// IEffects may assume that their caller will only call them if there is something new to calculate. So, memoizing
/// the implementation of Apply is not necessary.
/// </summary>
public interface IEffect
{
    void Apply(EffectContext context, string inputPath, string outputPath)
    {
        File.Copy(inputPath, outputPath);
    }
}
