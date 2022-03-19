using System;
using System.Collections.Generic;
using System.IO;
using FFMpegCore;

namespace Vdcrpt
{
    /// <summary>
    /// A Video contains raw AVI video data and provides methods for manipulating it. It simplifies import and export
    /// from formats supported by FFMpeg.
    /// </summary>
    public class Video
    {
        private readonly List<byte> _videoData;

        private Video(List<byte> videoData)
        {
            _videoData = videoData;
        }

        // TODO: Move to temp directory
        private static string GetCachedFilePath(string path)
        {
            return Path.ChangeExtension(path, ".vdcrpt-data");
        }

        /// <summary>
        /// Loads (and optionally caches) an input video from the given path. Converts the container to AVI for easy
        /// corruption.
        /// </summary>
        /// <param name="inputPath">Input video</param>
        /// <param name="videoCodec">FFMpeg-supported video codec to use for corruption</param>
        /// <param name="audioCodec">FFMpeg-supported audio codec to use for corruption</param>
        /// <returns></returns>
        public static Video Load(string inputPath, string videoCodec = "mpeg4", string audioCodec = "pcm_mulaw")
        {
            var outputPath = GetCachedFilePath(inputPath);

            if (!File.Exists(outputPath))
            {
                FFMpegArguments
                    .FromFileInput(inputPath)
                    .OutputToFile(outputPath, true, args => args
                        .WithVideoCodec("mpeg4")
                        .WithAudioCodec("pcm_mulaw")
                        .ForceFormat("avi"))
                    .ProcessSynchronously();
            }

            // TODO: Check if existing file is valid

            return new Video(new List<byte>(File.ReadAllBytes(outputPath)));
        }

        public static bool CacheExists(string inputPath)
        {
            return File.Exists(GetCachedFilePath(inputPath));
        }

        /// <summary>
        /// Mutates this Video's binary data using a given function.
        /// </summary>
        /// <param name="action">Function to apply to binary data blob</param>
        /// <returns>This Video for chaining</returns>
        public Video ModifyBytes(Action<List<byte>> action)
        {
            action(_videoData);
            return this;
        }

        /// <summary>
        /// Re-renders this video using a given video and audio codec.
        /// </summary>
        /// <param name="outputPath">Path to save video to, will be overwritten</param>
        /// <param name="videoCodec">FFMpeg-supported video codec to use for output</param>
        /// <param name="audioCodec">FFMpeg-supported audio codec to use for output</param>
        public void Save(string outputPath, string videoCodec = "libx264", string audioCodec = "aac")
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllBytes(tempFile, _videoData.ToArray());

            FFMpegArguments
                .FromFileInput(tempFile)
                .OutputToFile(outputPath, true, args => args
                    .WithVideoCodec(videoCodec)
                    .WithAudioCodec(audioCodec))
                .ProcessSynchronously();
        }
    }
}