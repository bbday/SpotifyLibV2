using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SpotifyLibrary.Connect;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyProto;

namespace SpotifyLibrary.Audio
{
    public abstract class AbsChunkedStream : Stream, IEquatable<AbsChunkedStream>
    {
        private int _pos = 0;
        private int _totalSize;
        private readonly byte[] _key;
        
        protected AbsChunkedStream(byte[] key, TrackOrEpisode? trackOrEpisode)
        {
            _key = key;
            TrackOrEpisode = trackOrEpisode;
            Decryptor = new AudioDecrypt(_key);
            PlaybackId = LocalStateWrapper.GeneratePlaybackId();
        }
        public string PlaybackId { get; }
        public IAudioDecrypt Decryptor
        {
            get;
            private set;
        }
        public bool Initialized => Buffer?.Length > 0;

        public byte[][]? Buffer { get; private set; }
        public bool[]? AvailableChunks { get; private set; }

        public abstract Task<byte[]> FetchChunk(int index);
        public abstract Task<int> Initialize();

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!Initialized)
            {
                var totalLength = AsyncContext.Run(Initialize);

                var chunks = (int)Math.Ceiling((float)totalLength / (float)AudioDecrypt.CHUNK_SIZE);
                AvailableChunks = new bool[chunks];
                Buffer = new byte[chunks][];
                _totalSize = totalLength;
            }
            int i = 0;
            while (true)
            {
                try
                {
                    var chunk = (int)Position / AudioDecrypt.CHUNK_SIZE;
                    var chunkOff = (int) Position % AudioDecrypt.CHUNK_SIZE;
                    try
                    {
                        if (!AvailableChunks![chunk])
                        {
                            var getChunk =
                                AsyncContext.Run(() => FetchChunk(chunk));
                            //Decrypt it.
                            Decryptor.DecryptChunk(chunk, getChunk);
                            Buffer![chunk] = getChunk;
                            AvailableChunks[chunk] = true;
                        }

                        var copy = Math.Min(Buffer![chunk].Length - chunkOff, count - i);

                        Array.Copy(Buffer[chunk],
                            chunkOff,
                            buffer,
                            offset + i, copy);
                        i += copy;
                        Position += copy;

                        if (i == count || Position >= Length)
                            return i;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return 0;
                    }
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
        public override bool CanWrite => false;

        public override long Position
        {
            get => _pos;
            set => _pos = (int)value;
        }
        public override long Length => _totalSize;
        public TrackOrEpisode? TrackOrEpisode { get; }
        public ISpotifyId Id => TrackOrEpisode?.id;

        protected override void Dispose(bool disposing)
        {
            Buffer = new byte[0][];
            base.Dispose(disposing);
        }

        public bool Equals(AbsChunkedStream? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Nullable.Equals(TrackOrEpisode, other.TrackOrEpisode);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AbsChunkedStream) obj);
        }

        public override int GetHashCode()
        {
            return TrackOrEpisode.GetHashCode();
        }
    }


    public class UrlStream : AbsChunkedStream
    {
        private readonly CdnUrl _cdnUrl;

        internal UrlStream(CdnUrl cdnUrl,
            byte[] key, TrackOrEpisode trackorepisode) : base(key, trackorepisode)
        {
            _cdnUrl = cdnUrl;
        }

        public override async Task<byte[]> FetchChunk(int index)
        {
            Debug.WriteLine($"Requested chunk {index}");
            var b = await GetRequest(_cdnUrl,
                AudioDecrypt.CHUNK_SIZE * index,
                (index + 1) * AudioDecrypt.CHUNK_SIZE - 1);
            return b.buffer;
        }

        public override async Task<int> Initialize()
        {
            var resp = await GetRequest(_cdnUrl,
                0,
                AudioDecrypt.CHUNK_SIZE - 1);

            var contentRange = resp.headers["Content-Range"];
            if (string.IsNullOrEmpty(contentRange))
                throw new Exception("Missing Content-Range header");
            var split = contentRange.Split('/');
            var size = int.Parse(split[1]);
            return size;
        }


        private async Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, long rangeStart, long rangeEnd)
        {
            var request = WebRequest.Create(await cdnUrl.Url()) as HttpWebRequest;
            Debug.Assert(request != null, nameof(request) + " != null");
            request.AddRange(rangeStart, rangeEnd);

            using var httpWebResponse = request.GetResponse() as HttpWebResponse;
            Debug.Assert(httpWebResponse != null, nameof(httpWebResponse) + " != null");
            if (httpWebResponse.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new Exception($"{httpWebResponse.StatusCode} : {httpWebResponse.StatusDescription}");
            }

            using var input = httpWebResponse.GetResponseStream();
            using var mem = new MemoryStream();
            Debug.Assert(input != null, nameof(input) + " != null");
            await input.CopyToAsync(mem);
            return (mem.ToArray(), httpWebResponse.Headers);
        }
    }

    public class FileStreamInternal : AbsChunkedStream
    {
        private readonly FileStream _fs;
        private readonly CancellationTokenSource _cts;
        internal FileStreamInternal(
            string filePath, byte[] key,
            TrackOrEpisode trackOrEpisode) : base(key, trackOrEpisode)
        {
            _cts = new CancellationTokenSource();
            _fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public override async Task<byte[]> FetchChunk(int index)
        {
            Debug.WriteLine($"Requested chunk {index}");
            var rangeStart = AudioDecrypt.CHUNK_SIZE * index;
            var rangeEnd = (index + 1) * AudioDecrypt.CHUNK_SIZE - 1;
            var length = rangeEnd - rangeStart + 1;
            var buffer = new byte[length];
            _fs.Seek(rangeStart, SeekOrigin.Begin);
            try
            {
                await _fs.ReadAsync(buffer, 0, length, _cts.Token);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == _cts.Token)
            {
                _cts?.Dispose();
                _fs?.Dispose();
            }
            catch (ObjectDisposedException disposed)
            {

            }
            return buffer;
        }

        public override async Task<int> Initialize()
        {
            return (int)_fs.Length;
        }

        protected override void Dispose(bool disposing)
        {
            _cts?.Cancel();
            base.Dispose(disposing);
        }
    }

    public class ByteStreamInternal : AbsChunkedStream
    {
        private readonly byte[] trackData;
        internal ByteStreamInternal(
            byte[] data, byte[] key,
            TrackOrEpisode trackOrEpisode) : base(key, trackOrEpisode)
        {
            trackData = data;
        }
        public override async Task<byte[]> FetchChunk(int index)
        {
            Debug.WriteLine($"Requested chunk {index}");
            var rangeStart = AudioDecrypt.CHUNK_SIZE * index;
            var rangeEnd = (index + 1) * AudioDecrypt.CHUNK_SIZE - 1;
            var length = rangeEnd - rangeStart + 1;
            var buffer =
                trackData.Skip(rangeStart)
                    .Take(length)
                    .ToArray();
            return buffer;
        }

        public override async Task<int> Initialize()
        {
            return (int) trackData.Length;
        }
    }
}
