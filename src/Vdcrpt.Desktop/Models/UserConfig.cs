using System.Collections.Generic;

namespace Vdcrpt.Desktop.Models;

public class UserConfig
{
    public bool OpenWhenComplete { get; set; }
    public bool AskForFilename { get; set; }
    public IReadOnlyList<Preset> Presets { get; private set; }

    public UserConfig()
    {
        Presets = new List<Preset>();
    }

    public static UserConfig CreateDefault()
    {
        return new UserConfig()
        {
            OpenWhenComplete = false,
            AskForFilename = true,

            Presets = new List<Preset>()
            {
                new()
                {
                    Name = "Melting Chaos",
                    Settings = new() { BurstSize = 3000,
                    MinBurstLength = 8,
                    Iterations = 400
                }},
                new()
                {
                    Name = "Jittery",
                    Settings = new() { BurstSize = 20000,
                    MinBurstLength = 1,
                    MaxBurstLength = 8,
                    Iterations = 200
                }},
                new()
                {
                    Name = "Source Engine",
                    Settings = new() { BurstSize = 45000,
                    MinBurstLength = 2,
                    MaxBurstLength = 6,
                    Iterations = 60
                }},
                new()
                {
                    Name = "Subtle",
                    Settings = new() { BurstSize = 200,
                    MinBurstLength = 2,
                    Iterations = 60
                }},
                new()
                {
                    Name = "Many Artifacts",
                    Settings = new() { BurstSize = 500,
                    MinBurstLength = 3,
                    Iterations = 2000
                }},
                new()
                {
                    Name = "Trash (unstable, breaks audio)",
                    Settings = new() { BurstSize = 1,
                    MinBurstLength = 1,
                    Iterations = 10000
                }},
                new()
                {
                    Name = "Legacy",
                    Settings = new() { BurstSize = 1000,
                    MinBurstLength = 10,
                    MaxBurstLength = 90,
                    Iterations = 50
                }},
            }
        };
    }
}
