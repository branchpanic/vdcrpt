using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vdcrpt.Avi
{
    internal static class FourCC
    {
        public const string Movi = "movi";
        public const string List = "LIST";
        public const string Junk = "JUNK";
        public const string Idx1 = "idx1";
    }

    internal static class BinaryReaderExtensions
    {
        public static string ReadFourCC(this BinaryReader reader)
        {
            return new string(reader.ReadChars(4));
        }
    }

    public struct FrameMetadata
    {
        public string Id;
        public int Flag;
        public long Offset;
        public long Size;
    }

    public sealed class AviFile : IDisposable, IAviManipulator
    {
        private long _moviPosition;
        private long _idx1Position;
        private List<FrameMetadata> _frames;

        public int FrameCount => _frames.Count;

        private string _tempFilePath;
        private FileStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public AviFile(string path)
        {
            _frames = new List<FrameMetadata>();
            _tempFilePath = path + "_vdcrpt_tmp";
            
            File.Copy(path, _tempFilePath, true);
            _stream = new FileStream(_tempFilePath, FileMode.Open);
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);

            // "RIFF", file size, "AVI "
            _stream.Seek(12, SeekOrigin.Begin);

            while (_reader.ReadFourCC() is FourCC.List or FourCC.Junk)
            {
                var chunkSize = _reader.ReadInt32();

                if (_reader.ReadFourCC() == FourCC.Movi)
                {
                    _moviPosition = _stream.Position - 4;
                }

                _stream.Seek(chunkSize - 4, SeekOrigin.Current);
            }

            _idx1Position = _stream.Position - 4;
            var idx1End = _reader.ReadInt32() + _stream.Position;
            var chunkId = _reader.ReadFourCC();

            while (_stream.Position < idx1End)
            {
                var frame = new FrameMetadata
                {
                    Id = chunkId,
                    Flag = _reader.ReadInt32(),
                    // TODO: Offset may be from movi _or_ start of file
                    Offset = _reader.ReadInt32(),
                    Size = _reader.ReadInt32()
                };

                _frames.Add(frame);
                chunkId = _reader.ReadFourCC();
            }
        }

        public void Save(string path, bool overwrite = false)
        {
            _stream.Flush(true);
            File.Copy(_tempFilePath, path, overwrite);
        }
        
        public void ForEachFrame(IAviManipulator.FrameAction action)
        {
            var rewriteStream = new MemoryStream();
            using var rewriteWriter = new BinaryWriter(rewriteStream);

            for (var i = 0; i < _frames.Count; i++)
            {
                var frame = _frames[i];
                
                SeekToFrameData(frame);
                var buf = _reader.ReadBytes((int) frame.Size);
                action(frame, ref buf);
                
                frame.Offset = rewriteStream.Position + 4;
                frame.Size = buf.Length;
                
                rewriteWriter.Write(frame.Id.ToCharArray());
                rewriteWriter.Write(buf.Length);
                rewriteWriter.Write(buf);

                if (buf.Length % 2 != 0)
                {
                    rewriteWriter.Write('\0');
                }
            }

            _stream.Seek(_moviPosition - 4, SeekOrigin.Begin);
            _writer.Write((int)(rewriteStream.Position + 4));
            _writer.Write(FourCC.Movi);
            
            rewriteStream.Seek(0, SeekOrigin.Begin);
            rewriteStream.WriteTo(_stream);
            
            Rewrite(rewriteStream);
        }

        private void Rewrite(MemoryStream data)
        {
            data.Seek(0, SeekOrigin.End);
            
            // movi
            _stream.Seek(_moviPosition - 4, SeekOrigin.Begin);
            _writer.Write((int)(data.Position + 4));
            _writer.Write(FourCC.Movi);
            data.Seek(0, SeekOrigin.Begin);
            data.WriteTo(_stream);
            
            // idx1
            _writer.Write(FourCC.Idx1);
            _writer.Write(_frames.Count * 16);
            
            foreach (var frame in _frames)
            {
                _writer.Write(frame.Id.ToCharArray());
                _writer.Write(frame.Flag);
                _writer.Write((int)frame.Offset);
                _writer.Write((int)frame.Size);
            }

            var eofPos = (int)_stream.Position;
            _stream.SetLength(eofPos);
            _stream.Seek(4, SeekOrigin.Begin);
            _writer.Write(eofPos - 8);

            _stream.Seek(48, SeekOrigin.Begin);
            var videoFrameCount = _frames.Count(f => f.Id.EndsWith("db") || f.Id.EndsWith("dc"));
            _writer.Write(videoFrameCount);
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _stream?.Dispose();
            File.Delete(_tempFilePath);
        }

        private void SeekToFrameData(int index) => SeekToFrameData(_frames[index]);

        private void SeekToFrameData(FrameMetadata frameMetadata)
        {
            _stream.Seek(_moviPosition + frameMetadata.Offset + 8, SeekOrigin.Begin);
        }
    }
}