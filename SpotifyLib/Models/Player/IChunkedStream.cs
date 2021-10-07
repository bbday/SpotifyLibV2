using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spotify.Metadata.Proto;
using SpotifyLib.Helpers;
using Debug = System.Diagnostics.Debug;

namespace SpotifyLib.Models.Player
{
    internal interface IFetcher : IDisposable
    {
        int TotalSize { get; }
        int Chunks { get; }
        bool[] Available { get; }
        byte[][] Buffer { get; }
        int GetChunk(int chunkIndex);
    }

    internal class FileFetcher : IFetcher
    {
        private FileStream _fs;

        private readonly string _path;

        public FileFetcher(string path)
        {
            _path = path;
            _fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
            TotalSize = (int) _fs.Length;
            Chunks = (int) Math.Ceiling((double) TotalSize / Consts.CHUNK_SIZE);
            Available = new bool[Chunks];
            Buffer = new byte[Chunks][];
        }

        public int TotalSize { get; }
        public int Chunks { get; }
        public bool[] Available { get; }
        public byte[][] Buffer { get; set; }

        public int GetChunk(int chunkIndex)
        {
            if (Buffer[chunkIndex] != null)
                return Buffer[chunkIndex].Length;

            _fs.Seek((long) chunkIndex * Consts.CHUNK_SIZE, SeekOrigin.Begin);
            var l = new byte[Consts.CHUNK_SIZE];
            Buffer[chunkIndex] = l;
            _fs.Read(Buffer[chunkIndex], 0, Consts.CHUNK_SIZE);
            Available[chunkIndex] = true;
            Debug.WriteLine($"Fetched chunk {chunkIndex} out of {Chunks}");
            Console.WriteLine($"Fetched chunk : {chunkIndex}");
            return l.Length;
        }

        public void Dispose()
        {
            _fs?.Dispose();
            Buffer = null;
        }
    }

    internal class CdnFetcher : IFetcher
    {
        private CdnUrl url;
        private SpotifyConnectionState _connState;
        private AudioDecrypt _decrypt;
        public CdnFetcher(CdnUrl url, byte[] audiokey,
            int totalSize,
            int chunks,
            byte[] firstChunk, SpotifyConnectionState connState)
        {
            this.url = url;
            TotalSize = totalSize;
            Chunks = chunks;
            _connState = connState;
            _decrypt = new AudioDecrypt(audiokey);
            Available = new bool[chunks];
            Buffer = new byte[chunks][];

            Available[0] = true;
            _decrypt.DecryptChunk(0, firstChunk);
            Buffer[0] = firstChunk;
        }


        public int TotalSize { get; private set; }
        public int Chunks { get; private set; }
        public bool[] Available { get; private set; }
        public byte[][] Buffer { get; private set; }
        public int GetChunk(int chunkIndex)
        {
            if (Available[chunkIndex]) return 0;

            var r = url
                .GetRequest(_connState,
                    Consts.CHUNK_SIZE * chunkIndex,
                    (chunkIndex + 1) * Consts.CHUNK_SIZE - 1)
                .Result;
            var k = r.Stream;
            Debug.WriteLine($"Fetching chunk {chunkIndex}");
            Console.WriteLine($"Fetching chunk {chunkIndex}");
            _decrypt.DecryptChunk(chunkIndex, k);
            Buffer[chunkIndex] = k;
            Available[chunkIndex] = true;
            return 0;
        }
        public void Dispose()
        {
            url = null;
            Buffer = null;
            Available = null;

        }
    }
    public class ChunkedStream : Stream, IDisposable
    {
        private IFetcher _fetcher;
        public ChunkedStream(string path,
            TrackOrEpisode metadata)
        {
            TrackOrEpisode = metadata;
            _fetcher = new FileFetcher(path);
            Length = _fetcher.TotalSize;
        }

        public ChunkedStream(SpotifyId id, byte[] bytes)
        {

        }

        public ChunkedStream(SpotifyId id, Stream stream)
        {

        }

        public ChunkedStream(SpotifyId id, CdnUrl url,
            Track track,
            byte[] audiokey,
            int totalSize,
            int chunks,
            byte[] firstChunk,
            SpotifyConnectionState connState)
        {
            _fetcher = new CdnFetcher(url, audiokey, totalSize,
                chunks,
                firstChunk, connState);
            Length = totalSize;
            TrackOrEpisode = new TrackOrEpisode(track, id);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {

            if (count == 0) return 0;
            if (pos >= Length) return -1;

            int i = 0;
            while (true)
            {
                try
                {
                    int chunk = pos / Consts.CHUNK_SIZE;
                    int chunkOff = pos % Consts.CHUNK_SIZE;

                    _fetcher.GetChunk(chunk);

                    int copy = Math.Min(_fetcher.Buffer[chunk].Length - chunkOff, count - i);
                    Array.Copy(_fetcher.Buffer[chunk], chunkOff,
                        buffer, offset + i, copy);
                    i += copy;
                    pos += copy;
                    //return copy;
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

        public override int ReadByte()
        {
            return base.ReadByte();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position += offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
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

        public override bool CanWrite => true;
        public override long Length { get; }

        public override long Position
        {
            get => pos;
            set => pos = (int)value;
        }


        public SpotifyId Id => TrackOrEpisode.Id;
        public TrackOrEpisode TrackOrEpisode { get; }

        public string PlaybackId = SpotifyConnectState.GeneratePlaybackId();
        private int pos;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _fetcher.Dispose();
            _fetcher = null;
        }
    }

    public readonly struct TrackOrEpisode : IEquatable<TrackOrEpisode>
    {
        public SpotifyId Id { get; }
        public Track Track { get; }

        public TrackOrEpisode(Track track, SpotifyId id)
        {
            Id = id;
            Track = track;
        }

        public bool Equals(TrackOrEpisode other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is TrackOrEpisode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
