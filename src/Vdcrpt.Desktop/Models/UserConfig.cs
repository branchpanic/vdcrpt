using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vdcrpt.Desktop.Models;

public partial class UserConfig : ObservableObject
{
    [ObservableProperty] private bool _askForFilename;
    [ObservableProperty] private bool _openWhenComplete;

    public UserConfig()
    {
        Presets = new List<Preset>();
    }

    public IReadOnlyList<Preset> Presets { get; private set; }

    public static UserConfig CreateDefault()
    {
        return new UserConfig
        {
            OpenWhenComplete = false,
            AskForFilename = true,

            Presets = new List<Preset>
            {
                new()
                {
                    Name = "Melting Chaos",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 3000,
                        MinBurstLength = 8,
                        Iterations = 400
                    }
                },
                new()
                {
                    Name = "Jittery",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 20000,
                        MinBurstLength = 1,
                        MaxBurstLength = 8,
                        UseBurstLengthRange = true,
                        Iterations = 200
                    }
                },
                new()
                {
                    Name = "Source Engine",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 45000,
                        MinBurstLength = 2,
                        MaxBurstLength = 6,
                        UseBurstLengthRange = true,
                        Iterations = 60
                    }
                },
                new()
                {
                    Name = "Subtle",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 200,
                        MinBurstLength = 2,
                        Iterations = 60
                    }
                },
                new()
                {
                    Name = "Many Artifacts",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 500,
                        MinBurstLength = 3,
                        Iterations = 2000
                    }
                },
                new()
                {
                    Name = "Trash (unstable, breaks audio)",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 1,
                        MinBurstLength = 1,
                        Iterations = 10000
                    }
                },
                new()
                {
                    Name = "Legacy",
                    Settings = new BinaryRepeatEffectSettings
                    {
                        BurstSize = 1000,
                        MinBurstLength = 10,
                        MaxBurstLength = 90,
                        UseBurstLengthRange = true,
                        Iterations = 50
                    }
                }
            }
        };
    }
}