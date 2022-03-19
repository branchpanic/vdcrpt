using System;
using System.IO;
using CommandLine;

namespace Vdcrpt.CommandLine
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Args
    {
        [Value(0, MetaName = "input", Required = true, HelpText = "Video to corrupt.")]
        public string InputFile { get; set; }

        [Value(1, MetaName = "output", Required = true, HelpText = "Output video.")]
        public string OutputFile { get; set; }

        [Option('o', "overwrite", HelpText = "Whether or not to overwrite existing outputs.", Default = false)]
        public bool Overwrite { get; set; }

        [Option('v', "vcodec", HelpText = "Video codec to use during corruption.", Default = "mpeg4")]
        public string VideoCodec { get; set; }

        [Option('a', "acodec", HelpText = "Audio codec to use during corruption.", Default = "pcm_mulaw")]
        public string AudioCodec { get; set; }

        [Option('i', "iterations", HelpText = "Number of iterations.", Default = 20)]
        public int Iterations { get; set; }

        [Option('s', "chunk-size", HelpText = "Repetition chunk size in bytes.", Default = 5000)]
        public int ChunkSize { get; set; }

        [Option('r', "min-repetitions", HelpText = "Minimum number of repetitions.", Default = 10)]
        public int MinRepetitions { get; set; }

        [Option('R', "min-repetitions", HelpText = "Maximum number of repetitions.", Default = 20)]
        public int MaxRepetitions { get; set; }
    }

    public static class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Args>(args).MapResult(parsedArgs =>
            {
                if (!parsedArgs.Overwrite && File.Exists(parsedArgs.OutputFile))
                {
                    Console.Error.WriteLine(
                        $"Output file {parsedArgs.OutputFile} already exists (use -o to overwrite)");
                    return 1;
                }

                var v = Video.Load(parsedArgs.InputFile, parsedArgs.VideoCodec, parsedArgs.AudioCodec);

                for (var i = 0; i < parsedArgs.Iterations; i++)
                {
                    v.ModifyBytes(Effects.Repeat(
                        parsedArgs.ChunkSize,
                        parsedArgs.MinRepetitions,
                        parsedArgs.MaxRepetitions));
                }

                v.Save(parsedArgs.OutputFile);

                return 0;
            }, _ => -1);
        }
    }
}