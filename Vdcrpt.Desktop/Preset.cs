namespace Vdcrpt.Desktop
{
    public class Preset
    {
        public string Name { get; init; } = string.Empty;
        public int TrailLength { get; init; }
        public int BurstSize { get; init; }
        public int Iterations { get; init; }
    }
}