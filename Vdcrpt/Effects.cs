using System;
using System.Collections.Generic;

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
        /// <param name="chunkLength">Length of video data to repeat</param>
        /// <param name="minTimes">Minimum times to repeat (inclusive</param>
        /// <param name="maxTimes">Maximum times to repeat (inclusive)</param>
        /// <returns>Function that applies the specified corruption</returns>
        public static Action<List<byte>> Repeat(int chunkLength, int minTimes, int maxTimes)
        {
            return data =>
            {
                var position = EffectRandom.Next(32, data.Count - chunkLength);
                var times = EffectRandom.Next(minTimes, maxTimes + 1);

                var clip = data.GetRange(position, chunkLength);

                for (var i = 0; i < times; i++)
                {
                    data.InsertRange(position, clip);
                }
            };
        }
    }
}