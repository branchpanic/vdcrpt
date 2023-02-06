using System.Collections.Generic;

namespace Vdcrpt.Desktop.Models;

public class Preset
{
    public string Name { get; init; }
    public BinaryRepeatEffectSettings Settings { get; set; }

    public Preset()
    {
        Name = string.Empty;
        Settings = new BinaryRepeatEffectSettings();
    }
}
