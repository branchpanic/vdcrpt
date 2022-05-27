using System;
using System.IO;
using CommandLine;
using Vdcrpt.Next;
using Vdcrpt.Next.Effects;

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
            return Parser.Default.ParseArguments<Args>(args).MapResult(Run, _ => -1);
        }

        private static int Run(Args args)
        {
            if (!args.Overwrite && File.Exists(args.OutputFile))
            {
                Console.Error.WriteLine($"Output file {args.OutputFile} already exists (use -o to overwrite)");
                return 1;
            }

            // var v = Video.Load(args.InputFile, args.VideoCodec, args.AudioCodec);
            //
            // v.Transform(Effects.Repeat(args.Iterations, args.ChunkSize, args.MinRepetitions, args.MaxRepetitions));
            //
            // v.Save(args.OutputFile);

            var effect = new BinaryRepeatEffect
            {
                Iterations = args.Iterations,
                BurstSize = args.ChunkSize,
                MaxBurstLength = args.MaxRepetitions,
                MinBurstLength = args.MinRepetitions
            };
            
            Session.ApplyEffects(args.InputFile, args.OutputFile, effect);

            return 0;
    }
    }
}