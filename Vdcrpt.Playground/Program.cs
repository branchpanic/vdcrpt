using System;
using Vdcrpt.Avi;

namespace Vdcrpt.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new Random();
            using var f = new AviFile("/Users/joe/Source/vdcrpt/flame.avi");
            f.ForEachFrame(((FrameMetadata metadata, ref byte[] data) =>
            {
            }));
            f.Save("/Users/joe/Source/vdcrpt/flame2.avi", true);
        }
    }
}