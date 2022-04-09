namespace Vdcrpt.Desktop
{
    public class Preset
    {
        public string Name { get; init; } = string.Empty;
        public bool RandomizeTrailLength { get; init; }
        public int MinTrailLength { get; init; }
        public int MaxTrailLength { get; init; }
        public int BurstSize { get; init; }
        public int Iterations { get; init; }
    }
}