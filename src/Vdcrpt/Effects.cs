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
        public static Action<List<byte>> Repeat(int iterations,
            int chunkSize,
            int minRepetitions,
            int maxRepetitions 
        )
        {
            return data =>
            {
                var bytes = data.ToArray();
                
                var repetitions = new int[iterations];
                if (minRepetitions == maxRepetitions)
                {
                    Array.Fill(repetitions, minRepetitions);
                }
                else
                {
                    for (var i = 0; i < iterations; i++)
                    {
                        repetitions[i] = EffectRandom.Next(minRepetitions, maxRepetitions + 1);
                    }
                }

                var positions = new int[iterations];
                for (var i = 0; i < iterations; i++)
                {
                    positions[i] = EffectRandom.Next(32, data.Count - chunkSize);
                }

                Array.Sort(positions);

                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);

                var lastEnd = 0;
                for (var i = 0; i < positions.Length; i++)
                {
                    var pos = positions[i];
                    writer.Write(bytes, lastEnd, pos - lastEnd);
                    
                    for (var j = 0; j < repetitions[i]; j++)
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