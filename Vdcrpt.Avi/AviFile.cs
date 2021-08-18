using System;
using System.IO;

namespace Vdcrpt.Avi
{
    // https://github.com/ucnv/aviglitch/blob/master/lib/aviglitch/frames.rb
    // https://docs.microsoft.com/en-us/windows/win32/directshow/avi-riff-file-reference
    
    public class AviFile
    {
        public AviFile(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            using var reader = new BinaryReader(stream);

            throw new NotImplementedException();
        }
    }
}