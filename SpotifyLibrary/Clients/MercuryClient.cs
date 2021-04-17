using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Spotify;
using SpotifyLibrary.Bases;
using SpotifyLibrary.Connection;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;

namespace SpotifyLibrary.Clients
{
    public class MercuryClient : PacketsManager, IMercuryClient
    {
        private static readonly int MercuryRequestTimeout = 3000;

        private readonly ConcurrentDictionary<long, ICallback> _callbacks =
            new();
        private readonly SpotifyLibrary _library;

        private readonly ConcurrentDictionary<long, BytesArrayList> _partials
            = new();

        private readonly EventWaitHandle _removeCallbackLock =
            new AutoResetEvent(false);

        private readonly List<InternalSubListener> _subscriptions =
            new();
        private volatile int _seqHolder;



        internal MercuryClient(SpotifyLibrary connection) : base("mercury")
        {
            _library = connection;

            Connection = new SpotifyConnection(connection);
        }

        public override void Dispose(bool dispose)
        {
            Connection?.Dispose();
        }

        protected override void Handle(MercuryPacket packet)
        {
            using var stream = new MemoryStream(packet.Payload);
            int seqLength = getShort(packet.Payload, (int)stream.Position, true);
            stream.Seek(2, SeekOrigin.Current);
            long seq = 0;
            var buffer = packet.Payload;
            switch (seqLength)
            {
                case 2:
                    seq = getShort(packet.Payload, (int)stream.Position, true);
                    stream.Seek(2, SeekOrigin.Current);
                    break;
                case 4:
                    seq = getInt(packet.Payload, (int)stream.Position, true);
                    stream.Seek(4, SeekOrigin.Current);
                    break;
                case 8:
                    seq = getLong(packet.Payload, (int)stream.Position, true);
                    stream.Seek(8, SeekOrigin.Current);
                    break;
            }

            var flags = packet.Payload[(int)stream.Position];
            stream.Seek(1, SeekOrigin.Current);
            var parts = getShort(packet.Payload, (int)stream.Position, true);
            stream.Seek(2, SeekOrigin.Current);

            _partials.TryGetValue(seq, out var partial);
            if (partial == null || flags == 0)
            {
                partial = new BytesArrayList();
                _partials.TryAdd(seq, partial);
            }

            Debug.WriteLine("Handling packet, cmd: " +
                            $"{packet.Cmd}, seq: {seq}, flags: {flags}, parts: {parts}");

            for (var j = 0; j < parts; j++)
            {
                var size = getShort(packet.Payload, (int)stream.Position, true);
                stream.Seek(2, SeekOrigin.Current);

                var buffer2 = new byte[size];

                var end = buffer2.Length;
                for (var z = 0; z < end; z++)
                {
                    var a = packet.Payload[(int)stream.Position];
                    stream.Seek(1, SeekOrigin.Current);
                    buffer2[z] = a;
                }

                partial.Add(buffer2);
            }

            if (flags != 1) return;

            _partials.TryRemove(seq, out partial);

            Header header;
            try
            {
                header = Header.Parser.ParseFrom(partial.First());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Couldn't parse header! bytes: {partial.First().BytesToHex()}");
                throw ex;
            }

            var resp = new MercuryResponse(header, partial);
            switch (packet.Cmd)
            {
                case MercuryPacketType.MercuryEvent:
                    var dispatched = false;
                    lock (_subscriptions)
                    {
                        foreach (var sub in _subscriptions)
                            if (sub.Matches(header.Uri))
                            {
                                sub.Dispatch(resp);
                                dispatched = true;
                            }
                    }

                    if (!dispatched)
                        Debug.WriteLine(
                            $"Couldn't dispatch Mercury event seq: {seq}, uri: {header.Uri}, code: {header.StatusCode}, payload: {resp.Payload.ToHex()}");

                    break;
                case MercuryPacketType.MercuryReq:
                case MercuryPacketType.MercurySub:
                case MercuryPacketType.MercuryUnsub:
                    _callbacks.TryRemove(seq, out var val);
                    if (val != null)
                        val.Response(resp);
                    else
                        Debug.WriteLine(
                            $"Skipped Mercury response, seq: {seq}, uri: {header.Uri}, code: {header.StatusCode}");

                    lock (_removeCallbackLock)
                    {
                        _removeCallbackLock.Reset();
                    }

                    break;
                default:
                    Debugger.Break();
                    break;
            }
        }

        protected override void Exception(Exception ex)
        {
            throw new NotImplementedException();
        }

        public ISpotifyConnection Connection { get; }
        public string CountryCode => Connection?.CountryCode;

        //Not truly async. Fix later
        public async Task<T> SendAsync<T>(SystemTextJsonMercuryRequest<T> request, CancellationToken? ct = null) where T : class
        {
            var resp = await SendAsync(request.Request, ct);
            if (resp.HasValue)
            {
                if (resp.Value.StatusCode >= 200 && resp.Value.StatusCode < 300) return request.Instantiate(resp.Value);
                throw new MercuryException(resp.Value);
            }
            else
            {
                throw new IOException("Cannot connect to mercury... Reconnecting");
            }
        }

        public async Task<T> SendAsync<T>(JsonMercuryRequest<T> request, CancellationToken? ct = null) where T : class
        {
            var resp = await SendAsync(request.Request, ct);
            if (resp.HasValue)
            {
                if (resp.Value.StatusCode >= 200 && resp.Value.StatusCode < 300) return request.Instantiate(resp.Value);
                throw new MercuryException(resp.Value);
            }
            else
            {
                throw new IOException("Cannot connect to mercury... Reconnecting");
            }
        }

        public async Task<MercuryResponse?> SendAsync(
            RawMercuryRequest request, CancellationToken? ct = null)
        {
            ct ??= CancellationToken.None;
            var callback = new SyncCallback(ct.Value);

            var seq = await Send(request, callback, ct.Value);
            if (seq != int.MinValue)
            {
                var resp = callback.WaitResponse();
                if (resp.Payload == null)
                    throw new Exception(
                        $"Request timeout out, {MercuryRequestTimeout} passed, yet no response. seq: {seq}");
                return resp;
            }

            return null;
        }
        public async Task<int> Send(RawMercuryRequest request, ICallback callback,
            CancellationToken ct)
        {
            if (!Connection.IsConnected)
            {
                var response = await Connection.Connect(_library.Authenticator);
                if (!response.Success)
                {
                    if (response.Exception != null)
                        throw response.Exception;
                    else
                        return int.MinValue;
                }
            }

            var partial = new List<byte[]>();
            var watch = Stopwatch.StartNew();
            var payloadNew = new List<byte[]>();
            var requestPayload = request._payload.ToArray();
            var requestHeader = request._header;
            if (requestPayload == null || requestHeader == null)
                throw new Exception("An unknown error occured. the librar could be outdated");

            var bytesOut = new MemoryStream();
            var s4B = BitConverter.GetBytes((short)4).Reverse().ToArray();
            bytesOut.Write(s4B, 0, s4B.Length); // Seq length


            var seqB = BitConverter.GetBytes(Interlocked.Increment(ref _seqHolder)).Reverse().ToArray();
            bytesOut.Write(seqB, 0, seqB.Length); // Seq

            bytesOut.WriteByte(1); // Flags
            var reqpB = BitConverter.GetBytes((short)(1 + requestPayload.Length)).Reverse().ToArray();
            bytesOut.Write(reqpB, 0, reqpB.Length); // Parts count

            var headerBytes2 = requestHeader.ToByteArray();
            var hedBls = BitConverter.GetBytes((short)headerBytes2.Length).Reverse().ToArray();

            bytesOut.Write(hedBls, 0, hedBls.Length); // Header length
            bytesOut.Write(headerBytes2, 0, headerBytes2.Length); // Header

            foreach (var part in requestPayload)
            {
                // Parts
                var l = BitConverter.GetBytes((short)part.Length).Reverse().ToArray();
                bytesOut.Write(l, 0, l.Length);
                bytesOut.Write(part, 0, part.Length);
            }

            var cmd = request._header.Method.ToLower() switch
            {
                "sub" => MercuryPacketType.MercurySub,
                "unsub" => MercuryPacketType.MercuryUnsub,
                _ => MercuryPacketType.MercuryReq
            };
            Connection.Send(cmd, bytesOut.ToArray(), ct);
            _callbacks.TryAdd(_seqHolder, callback);
            return _seqHolder;
        }
        #region PRivates

        private static short getShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short)(!obj2 ? (int)getShortL(obj0, obj1) : (int)getShortB(obj0, obj1));
        }

        private static short getShortB(byte[] obj0, int obj1)
        {
            return makeShort(obj0[obj1], obj0[obj1 + 1]);
        }

        private static short getShortL(byte[] obj0, int obj1)
        {
            return makeShort(obj0[obj1 + 1], obj0[obj1]);
        }

        private static short makeShort(byte obj0, byte obj1)
        {
            return (short)(((sbyte)obj0 << 8) | ((sbyte)obj1 & byte.MaxValue));
        }

        private static int getInt(byte[] obj0, int obj1, bool obj2)
        {
            return !obj2 ? getIntL(obj0, obj1) : getIntB(obj0, obj1);
        }

        private static int getIntB(byte[] obj0, int obj1)
        {
            return makeInt(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3]);
        }

        private static int getIntL(byte[] obj0, int obj1)
        {
            return makeInt(obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);
        }

        private static int makeInt(byte obj0, byte obj1, byte obj2, byte obj3)
        {
            return ((sbyte)obj0 << 24) | (((sbyte)obj1 & byte.MaxValue) << 16) |
                   (((sbyte)obj2 & byte.MaxValue) << 8) | ((sbyte)obj3 & byte.MaxValue);
        }

        private static long getLong(byte[] obj0, int obj1, bool obj2)
        {
            return !obj2 ? getLongL(obj0, obj1) : getLongB(obj0, obj1);
        }

        private static long getLongB(byte[] obj0, int obj1)
        {
            return makeLong(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3], obj0[obj1 + 4], obj0[obj1 + 5],
                obj0[obj1 + 6], obj0[obj1 + 7]);
        }

        private static long getLongL(byte[] obj0, int obj1)
        {
            return makeLong(obj0[obj1 + 7], obj0[obj1 + 6], obj0[obj1 + 5], obj0[obj1 + 4], obj0[obj1 + 3],
                obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);
        }

        private static long makeLong(
            byte obj0,
            byte obj1,
            byte obj2,
            byte obj3,
            byte obj4,
            byte obj5,
            byte obj6,
            byte obj7)
        {
            return ((long)(sbyte)obj0 << 56)
                   | (((sbyte)obj1 & (long)byte.MaxValue) << 48)
                   | (((sbyte)obj2 & (long)byte.MaxValue) << 40)
                   | (((sbyte)obj3 & (long)byte.MaxValue) << 32)
                   | (((sbyte)obj4 & (long)byte.MaxValue) << 24)
                   | (((sbyte)obj5 & (long)byte.MaxValue) << 16)
                   | (((sbyte)obj6 & (long)byte.MaxValue) << 8)
                   | ((sbyte)obj7 & (long)byte.MaxValue);
        }

        #endregion
    }
}