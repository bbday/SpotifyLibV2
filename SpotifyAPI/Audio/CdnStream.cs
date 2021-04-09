using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using SpotifyLibrary.Audio.KeyStuff;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Player;
using SpotifyLibrary.Services;
using SpotifyLibrary.Services.Mercury;
using SpotifyProto;

namespace SpotifyLibrary.Audio
{
    public static class CacheConstants
    {
        public static string TRACK_CHUNK = "TRACK_CHUNK";
        public static string TRACK_COMPLETELY_CACHED = "TRACK-CACHED-{0}";
    }
    public class CdnStream : AbsChunkedInputStream, IGeneralAudioStream
    {
        public override string ToString() => _file.FileId.ToBase64();

        private readonly int _totalSize;
        private readonly IAudioDecrypt _audioDecrypt;

        private readonly AudioFile _file;
        private Uri _url;
        private readonly int _chunks;
        private SpotifyConfiguration _configuration;
        private readonly bool fromCache;
        private byte[][] _buffer;
        private bool[] _requested;
        private bool[] _available;
        private readonly IHaltListener _haltListener;
        private readonly ICdnManager _cdnManager;
        private readonly CdnUrl _cdnUrl;

        private readonly ISpotifyPlayer _player;
        private readonly ISpotifyId _id;
        private readonly ICacheManager _cacheManager;
        public CdnStream(ISpotifyId id, AudioFile file,
            Uri url,
            int size,
            int chunks,
            SpotifyConfiguration configuration,
            byte[] firstChunk,
            bool preloaded,
            IHaltListener haltListener,
            IAudioDecrypt audioDecrypt,
            ICdnManager cdnManager,
            CdnUrl cdnurl,
            ISpotifyPlayer player,
            ICacheManager cacheManager,

            Track track,
            Episode episode)
            : base(false)
        {
            Track = track;
            Episode = episode;
            Id = id;
            FileId = file.FileId;
            _id = id;
            _cacheManager = cacheManager;
            _file = file;
            this._url = url;
            _chunks = chunks;
            retries = new int[_chunks];
            this._configuration = configuration;
            fromCache = preloaded;
            _haltListener = haltListener;
            _audioDecrypt = audioDecrypt;
            _cdnManager = cdnManager;
            _cdnUrl = cdnurl;
            _player = player;
            _totalSize = size;
            _available = new bool[chunks];
            _requested = new bool[chunks];
            _buffer = new byte[chunks][];
            _requested[0] = true;
            _ = WriteChunk(firstChunk, 0, fromCache);
        }
        public override void StreamReadHalted(int chunk, long time)
            => _haltListener.StreamReadHalted(chunk, time);

        public override void StreamReadResumed(int chunk, long time) =>
            _haltListener.StreamReadResumed(chunk, time);
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (closed) return 0;
            int i = 0;
            while (true)
            {
                try
                {
                    int chunk = pos / ChannelManager.CHUNK_SIZE;
                    int chunkOff = pos % ChannelManager.CHUNK_SIZE;

                    CheckAvailability(chunk, true, false)
                        .ConfigureAwait(false)
                        .GetAwaiter().GetResult();

                    int copy = Math.Min(Buffer()[chunk].Length - chunkOff, count - i);
                    Array.Copy(Buffer()[chunk], chunkOff, buffer, offset + i, copy);
                    i += copy;
                    pos += copy;

                    if (i == count || pos >= Size)
                        return i;
                }
                catch(Exception x)
                {
                    Debug.WriteLine(x.ToString());
                }
            }
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

        public override long Length => _totalSize;

        public override long Position
        {
            get => pos;
            set => pos = (int)value;
        }


        public override int Size
        {
            get => _totalSize;
            set => throw new NotFiniteNumberException();
        }

        protected override bool[] RequestedChunks() => _requested;
        protected override bool[] AvailableChunks() => _available;
        protected override int Chunks() => _chunks;

        protected override Task RequestChunkFromStream(int index) => RequestChunk(index);
        protected override byte[][] Buffer() => _buffer;


        public NormalizationData NormalizationData { get; set; }
        public int DecryptTimeMs() => _audioDecrypt.DecryptTimeMs();

        public Stream Stream() => this;
        public ByteString FileId { get; }
        public ISpotifyId Id { get; }
        public Episode Episode { get; }
        public Track Track { get; }

        private async Task RequestChunk(int index)
        {
            if (_cacheManager != null)
            {
                var get =
                    await _cacheManager?.TryGetChunk(_id!, index!);
                if (!get.exists)
                {
                    var resp = await _cdnManager.GetRequest(_cdnUrl, index);
                    await WriteChunk(resp.buffer, index, false);
                }
                else
                {
                    await WriteChunk(get.chunk, index, true);
                }
            }
            else
            {
                var resp = await _cdnManager.GetRequest(_cdnUrl, index);
                await WriteChunk(resp.buffer, index, false);
            }
        }

        private async Task WriteChunk(byte[] chunk,
            int chunkIndex,
            bool cached)
        {
            if (closed) return;
            _buffer[chunkIndex] = chunk;

            if (cached)
            {
                //TODO: write to cache
                Debug.WriteLine($"CACHED Chunk {chunkIndex + 1}/{_chunks} completed," +
                $" cached: {cached}," +
                    $" stream: {ToString()}");
            }
            else
            {
                Debug.WriteLine($"Chunk {chunkIndex + 1}/{_chunks} completed," +
                                $" cached: {cached}," +
                                $" stream: {ToString()}");
                _audioDecrypt.DecryptChunk(chunkIndex, chunk);

                // if (chunkIndex < _chunks - 1)
                //  {
                // var t = new MemoryStream(chunk);
                //var j = t.Skip(0xa7);
                //var bo = j == 0xa7;

                //}
            }
            if (_cacheManager != null && _cacheManager.AllowCacheOfKey(CacheConstants.TRACK_CHUNK))
            {
                Task.Run(() => _player.ChunkReceived(_id, chunk, chunkIndex,
                    _chunks,
                    false));
            }
            NotifyChunkAvailable(chunkIndex);
        }
        private void NotifyChunkAvailable(int index)
        {
            AvailableChunks()[index] = true;
            _decodedLength += Buffer()[index].Length;

            lock (waitLock)
            {
                if (index == waitForChunk && !closed)
                {
                    waitForChunk = -1;
                    waitLock.Set();
                }
            }
        }
        private int _decodedLength;

        public override void Dispose(bool v)
        {
            closed = true;
            _buffer = null;
            _available = null;
            _requested = null;
            base.Dispose(v);
        }
    }
}
