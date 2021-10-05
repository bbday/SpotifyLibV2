using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SpotifyLib.Models.Player
{
    internal interface IFetcher : IDisposable
    {
        int TotalSize { get; }
        int Chunks { get; }
        bool[] Available { get; }
        bool[] Requested { get; }
        byte[][] Buffer { get; }
        int GetChunk(int chunkIndex);
    }

    internal class FileFetcher : IFetcher
    {
        public static readonly int CHUNK_SIZE = 2 * 128 * 1024;
        private FileStream _fs;

        private readonly string _path;

        public FileFetcher(string path)
        {
            _path = path;
            _fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
            TotalSize = (int) _fs.Length;
            Chunks = (int) Math.Ceiling((double) TotalSize / CHUNK_SIZE);
            Available = new bool[Chunks];
            Requested = new bool[Chunks];
            Buffer = new byte[Chunks][];
        }

        public int TotalSize { get; }
        public int Chunks { get; }
        public bool[] Available { get; }
        public bool[] Requested { get; }
        public byte[][] Buffer { get; set; }
        public int GetChunk(int chunkIndex)
        {
            if (Buffer[chunkIndex] != null)
                return Buffer[chunkIndex].Length;
            //var chunk = Position / FileFetcher.CHUNK_SIZE;
            //var chunkOff = Position % FileFetcher.CHUNK_SIZE;
            var toRead = Math.Min(CHUNK_SIZE, TotalSize - CHUNK_SIZE * chunkIndex);
            var bf = new byte[toRead];
            var n = _fs.Read(bf, 0, toRead);
            Buffer[chunkIndex] = bf;
            Requested[chunkIndex] = true;
            Available[chunkIndex] = true;
            Console.WriteLine($"Fetched chunk : {chunkIndex}");
            return n;
        }

        public void Dispose()
        {
            _fs?.Dispose();
            Buffer = null;
        }
    }

    public class ChunkedStream : Stream
    {
        private readonly IFetcher _fetcher;
        public ChunkedStream(SpotifyId id, string path)
        {
            Id = id;
            _fetcher = new FileFetcher(path);
        }

        public ChunkedStream(SpotifyId id,byte[] bytes)
        {

        }

        public ChunkedStream(SpotifyId id,Stream stream)
        {

        }

        public ChunkedStream(SpotifyId id, Uri url)
        {

        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i = 0;
            while (true)
            {
                try
                {
                    int chunk = pos / FileFetcher.CHUNK_SIZE;
                    int chunkOff = pos % FileFetcher.CHUNK_SIZE;

                    var f = _fetcher.GetChunk(chunk);

                    int copy = Math.Min(_fetcher.Buffer[chunk].Length - chunkOff, count - i);
                    Array.Copy(_fetcher.Buffer[chunk], chunkOff, 
                        buffer, offset + i, copy);
                    i += copy;
                    pos += copy;


                    if (i == count || Position >= _fetcher.TotalSize)
                        return i;
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    return 0;
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Position = offset;
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite { get; }
        public override long Length => _fetcher.TotalSize;
        public override long Position
        {
            get => pos;
            set => pos = (int)value;
        }

        public SpotifyId Id { get; }
        public string PlaybackId = SpotifyConnectState.GeneratePlaybackId();
        private int pos;
    }
}
