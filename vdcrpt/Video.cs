using System;
using System.Collections.Generic;
using System.IO;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace Vdcrpt
{
    public class Video
    {
        private readonly List<byte> _videoData;

        public Video(List<byte> videoData)
        {
            _videoData = videoData;
        }

        private static string GetCachedFilePath(string path)
        {
            return Path.ChangeExtension(path, ".vdcrpt-data");
        }

        public static Video Prepare(string inputPath, string videoCodec = "mpeg4", string audioCodec = "pcm_mulaw")
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

        public Video ModifyBytes(Action<List<byte>> action)
        {
            action(_videoData);
            return this;
        }

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