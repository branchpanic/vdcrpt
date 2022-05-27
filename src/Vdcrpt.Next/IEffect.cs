namespace Vdcrpt.Next;

/// <summary>
/// An IEffect is a process that takes a video file and produces another video file.
///
/// IEffects are heavier than traditional video filters. They will typically operate on a video file at the binary
/// level to intentionally break encoding somehow.
/// </summary>
public interface IEffect
{
    void Apply(EffectContext context, string inputPath, string outputPath)
    {
        File.Copy(inputPath, outputPath);
    }
}
