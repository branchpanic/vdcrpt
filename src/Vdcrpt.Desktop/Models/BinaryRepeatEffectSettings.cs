using Vdcrpt.BuiltIns.Effects;

namespace Vdcrpt.Desktop.Models
{
    public class BinaryRepeatEffectSettings
    {
        public int Iterations { get; set; }
        public int MinBurstLength { get; set; }
        public int? MaxBurstLength { get; set; } = null;
        public int BurstSize { get; set; }

        public BinaryRepeatEffect ToEffectInstance() => new BinaryRepeatEffect()
        {
            Iterations = Iterations,
            MinBurstLength = MinBurstLength,
            MaxBurstLength = MaxBurstLength,
            BurstSize = BurstSize,
        };
    }
}
