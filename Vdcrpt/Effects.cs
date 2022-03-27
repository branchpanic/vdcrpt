using System;
using System.Collections.Generic;
using System.IO;

namespace Vdcrpt
{
    /// <summary>
    /// Effects contains miscellaneous functions that generate Actions to use with Video.ModifyBytes.
    /// </summary>
    public static class Effects
    {
        private static readonly Random EffectRandom = new();

        /// <summary>
        /// Returns an effect that repeats one chunk of video data,
        /// chunkLength bytes long, somewhere between minTimes and maxTimes
        /// times.
        /// </summary>
        /// <returns>Function that applies the specified corruption</returns>
        public static Action<List<byte>> Repeat(int iterations, int chunkSize, int chunkRepetitions)
        {
            return data =>
            {
                var bytes = data.ToArray();
                var positions = new int[iterations];
                for (var i = 0; i < iterations; i++)
                {
                    positions[i] = EffectRandom.Next(32, data.Count - chunkSize);
                }

                Array.Sort(positions);

                // TODO: This is transplanted from a context where the result was being written directly to disk
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);

                var lastEnd = 0;
                foreach (var pos in positions)
                {
                    writer.Write(bytes, lastEnd, pos - lastEnd);
                    for (var j = 0; j < chunkRepetitions; j++)
                    {
                        writer.Write(bytes, pos, chunkSize);
                    }

                    lastEnd = pos;
                }

                writer.Write(bytes, lastEnd, data.Count - lastEnd);

                data.Clear();
                data.AddRange(stream.ToArray());
            };
        }
    }
}