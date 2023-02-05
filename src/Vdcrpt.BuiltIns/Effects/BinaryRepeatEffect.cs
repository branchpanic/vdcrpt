namespace Vdcrpt.BuiltIns.Effects;

/// <summary>
/// A BinaryRepeatEffect creates randomized bursts of repeated binary data in a file, kind of like a scratched CD or
/// stuck vinyl record.
/// </summary>
public class BinaryRepeatEffect : IEffect
{
    public int Iterations { get; init; }
    public int MinBurstLength { get; init; }
    public int MaxBurstLength { get; init; }
    public int BurstSize { get; init; }

    private Random _random;

    public BinaryRepeatEffect()
    {
        _random = new Random();
    }

    public void Apply(EffectContext context, string inputPath, string outputPath)
    {
        var aviPath = context.ConvertFile(inputPath, "mpeg4", "pcm_mulaw", "avi");
        var data = File.ReadAllBytes(aviPath);

        var repetitions = new int[Iterations];
        if (MinBurstLength <= MaxBurstLength)
        {
            Array.Fill(repetitions, MinBurstLength);
        }
        else
        {
            for (var i = 0; i < Iterations; i++)
            {
                repetitions[i] = _random.Next(MinBurstLength, MaxBurstLength + 1);
            }
        }

        var positions = new int[Iterations];
        for (var i = 0; i < Iterations; i++)
        {
            positions[i] = _random.Next(32, data.Length - BurstSize);
        }

        Array.Sort(positions);

        using var writer = new BinaryWriter(File.OpenWrite(outputPath));

        var lastEnd = 0;
        for (var i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            writer.Write(data, lastEnd, pos - lastEnd);

            for (var j = 0; j < repetitions[i]; j++)
            {
                writer.Write(data, pos, BurstSize);
            }

            lastEnd = pos;
        }

        writer.Write(data, lastEnd, data.Length - lastEnd);
    }
}