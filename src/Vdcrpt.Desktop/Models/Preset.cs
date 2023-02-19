namespace Vdcrpt.Desktop.Models
{
    public class Preset
    {
        public Preset()
        {
            Name = string.Empty;
            Settings = new BinaryRepeatEffectSettings();
        }

        public string Name { get; init; }
        public BinaryRepeatEffectSettings Settings { get; set; }
    }
}
