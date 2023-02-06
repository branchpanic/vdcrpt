using System.IO;

namespace Vdcrpt.Desktop.Models;

public class Project
{
    public string InputFile { get; set; }
    public UserConfig Config { get; set; }
    public BinaryRepeatEffectSettings EffectSettings { get; set; }

    public Project()
    {
        InputFile = string.Empty;
        Config = new UserConfig();
        EffectSettings = new BinaryRepeatEffectSettings();
    }
}
