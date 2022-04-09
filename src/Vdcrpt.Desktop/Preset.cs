using System.Collections.Generic;

namespace Vdcrpt.Desktop
{
    public class Preset
    {
        public string Name { get; init; } = string.Empty;
        public int BurstSize { get; init; }
        public int Iterations { get; init; }

        // TODO: Range was tacked on at the last minute, this can be
        // represented more succinctly. Some of this is going to get blown up
        // for user presets anyway, so it's fine for now.
        //
        // When not using range, MinBurstLength is the constant burst length.
        public bool UseLengthRange { get; init; }
        public int MinBurstLength { get; init; }
        public int MaxBurstLength { get; init; }

        // TODO: User-defined presets, this will become data
        public static List<Preset> DefaultPresets => new()
        {
            new Preset { Name = "Melting Chaos", BurstSize = 3000, MinBurstLength = 8, Iterations = 400 },
            new Preset
            {
                Name = "Jittery", BurstSize = 20000, MinBurstLength = 1, MaxBurstLength = 8,
                UseLengthRange = true, Iterations = 200
            },
            new Preset
            {
                Name = "Source Engine", BurstSize = 45000, MinBurstLength = 2, MaxBurstLength = 6,
                UseLengthRange = true, Iterations = 60
            },
            new Preset { Name = "Subtle", BurstSize = 200, MinBurstLength = 2, Iterations = 60 },
            new Preset { Name = "Many Artifacts", BurstSize = 500, MinBurstLength = 3, Iterations = 2000 },
            new Preset
            {
                Name = "Trash (unstable, breaks audio)", BurstSize = 1, MinBurstLength = 1, Iterations = 10000
            },
            new Preset
            {
                Name = "Legacy", BurstSize = 1000, MinBurstLength = 10, MaxBurstLength = 90,
                UseLengthRange = true, Iterations = 50
            },
        };
    }
}