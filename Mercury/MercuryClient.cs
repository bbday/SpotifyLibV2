using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify;
using SpotifyLibV2.Abstractions;
using SpotifyLibV2.Callbacks;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Listeners;

namespace SpotifyLibV2.Mercury
{
    public class MercuryClient : PacketsManager, IMercuryClient
    {
        private volatile int _seqHolder;
        private readonly ConcurrentDictionary<long, BytesArrayList> _partials
            = new ConcurrentDictionary<long, BytesArrayList>();
        private readonly List<InternalSubListener> _subscriptions =
            new List<InternalSubListener>();
        private readonly System.Threading.EventWaitHandle _removeCallbackLock = new System.Threading.AutoResetEvent(false);
        private readonly ConcurrentDictionary<long, ICallback> _callbacks = new ConcurrentDictionary<long, ICallback>();
        private static readonly int MercuryRequestTimeout = 3000;
        private readonly ISpotifyStream _stream;
        internal MercuryClient(ISpotifyStream stream)
            : base("pm-session")
        {
            _stream = stream;
            _seqHolder = 1;
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
            Debug.WriteLine($"Handling packet, cmd: " +
                            $"{packet.Cmd}, seq: {seq}, flags: {flags}, parts: {parts}");

            for (int j = 0; j < parts; j++)
            {
                short size = getShort(packet.Payload, (int)stream.Position, true);
                stream.Seek(2, SeekOrigin.Current);

                byte[] buffer2 = new byte[size];

                int end = buffer2.Length;
                for (int z = 0; z < end; z++)
                {
                    byte a = packet.Payload[(int)stream.Position];
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
                    bool dispatched = false;
                    lock (_subscriptions)
                    {
                        foreach (var sub in _subscriptions)
                        {
                            if (sub.Matches(header.Uri))
                            {
                                sub.Dispatch(resp);
                                dispatched = true;
                            }
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
                    {
                        val.Response(resp);
                    }
                    else
                    {
                        Debug.WriteLine(
                            $"Skipped Mercury response, seq: {seq}, uri: {header.Uri}, code: {header.StatusCode}");
                    }

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

        public void InterestedIn(string uri, ISubListener listener)
        {
            lock (_subscriptions)
            {
                _subscriptions.Add(new InternalSubListener(uri, listener, false));
            }
        }

        public MercuryResponse Subscribe(string uri, ISubListener listener)
        {
            var response = SendSync(RawMercuryRequest.Sub(uri));
            if (response.StatusCode != 200) throw new PubSubException(response.StatusCode);

            if (response.Payload.Any())
            {
                foreach (var payload in response.Payload)
                {
                    var sub = Subscription.Parser.ParseFrom(payload);
                    lock (_subscriptions)
                    {
                        _subscriptions.Add(new InternalSubListener(sub.Uri, listener, true));
                    }
                }
            }
            else
            {
                lock (_subscriptions)
                {
                    _subscriptions.Add(new InternalSubListener(uri, listener, true));
                }
            }

            Debug.WriteLine($"Subscribed successfully to {uri}!");
            return response;
        }

        public void Unsubscribe(string uri)
        {
            var response = SendSync(RawMercuryRequest.Unsub(uri));
            if (response.StatusCode != 200) throw new PubSubException(response.StatusCode);

            //_subscriptions.
            lock (_subscriptions)
            {
                var find = _subscriptions.FindIndex(k => k.Matches(uri));
                if (find != -1)
                {
                    _subscriptions.RemoveAt(find);
                }
            }

            Debug.WriteLine($"Unsubscribed successfully from {uri}!");
        }

        public MercuryResponse SendSync(RawMercuryRequest request)
        {
            var callback = new SyncCallback();

            int seq = Send(request, callback);
            var resp = callback.WaitResponse();
            if (resp == null)
                throw new Exception(
                    $"Request timeout out, {MercuryRequestTimeout} passed, yet no response. seq: {seq}");
            return resp;
        }

        public T SendSync<T>(JsonMercuryRequest<T> request) where T : class
        {
            var resp = SendSync(request.Request);
            if (resp.StatusCode >= 200 && resp.StatusCode < 300) return request.Instantiate(resp);
            else throw new MercuryException(resp);
        }

        public T SendSync<T>(ProtobuffedMercuryRequest<T> request) where T : IMessage<T>
        {
            var resp = SendSync(request.Request);
            if (resp.StatusCode >= 200 && resp.StatusCode < 300) return request.Instantiate(resp);
            else throw new MercuryException(resp);
        }

        public int Send(RawMercuryRequest request, ICallback callback)
        {
            var partial = new List<byte[]>();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var payloadNew = new List<byte[]>();
            var requestPayload = request._payload.ToArray();
            var requestHeader = request._header;
            if (requestPayload == null || requestHeader == null)
                throw new Exception("An unknown error occured. the librar could be outdated");

            var bytesOut = new MemoryStream();
            var s4B = (BitConverter.GetBytes((short)4)).Reverse().ToArray();
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

            foreach (byte[] part in requestPayload)
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
            _stream.Send(cmd, bytesOut.ToArray(), CancellationToken.None);
            _callbacks.TryAdd((long)_seqHolder, callback);
            return _seqHolder;
        }


        #region PRivates

        private static short getShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short)(!obj2 ? (int)getShortL(obj0, obj1) : (int)getShortB(obj0, obj1));
        }
        private static short getShortB(byte[] obj0, int obj1) => makeShort(obj0[obj1], obj0[obj1 + 1]);

        private static short getShortL(byte[] obj0, int obj1)
        {
            return makeShort(obj0[obj1 + 1], obj0[obj1]);
        }
        private static short makeShort(byte obj0, byte obj1) => (short)((int)(sbyte)obj0 << 8 | (int)(sbyte)obj1 & (int)byte.MaxValue);

        private static int getInt(byte[] obj0, int obj1, bool obj2) => !obj2 ? getIntL(obj0, obj1) : getIntB(obj0, obj1);

        private static int getIntB(byte[] obj0, int obj1) => makeInt(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3]);

        private static int getIntL(byte[] obj0, int obj1) => makeInt(obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);

        private static int makeInt(byte obj0, byte obj1, byte obj2, byte obj3) => (int)(sbyte)obj0 << 24 | ((int)(sbyte)obj1 & (int)byte.MaxValue) << 16 | ((int)(sbyte)obj2 & (int)byte.MaxValue) << 8 | (int)(sbyte)obj3 & (int)byte.MaxValue;

        private static long getLong(byte[] obj0, int obj1, bool obj2) => !obj2 ? getLongL(obj0, obj1) : getLongB(obj0, obj1);

        private static long getLongB(byte[] obj0, int obj1) => makeLong(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3], obj0[obj1 + 4], obj0[obj1 + 5], obj0[obj1 + 6], obj0[obj1 + 7]);

        private static long getLongL(byte[] obj0, int obj1) => makeLong(obj0[obj1 + 7], obj0[obj1 + 6], obj0[obj1 + 5], obj0[obj1 + 4], obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);

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
            return (long)(sbyte)obj0 << 56
                   | ((long)(sbyte)obj1 & (long)byte.MaxValue) << 48
                   | ((long)(sbyte)obj2 & (long)byte.MaxValue) << 40
                   | ((long)(sbyte)obj3 & (long)byte.MaxValue) << 32
                   | ((long)(sbyte)obj4 & (long)byte.MaxValue) << 24
                   | ((long)(sbyte)obj5 & (long)byte.MaxValue) << 16
                   | ((long)(sbyte)obj6 & (long)byte.MaxValue) << 8
                   | (long)(sbyte)obj7 & (long)byte.MaxValue;
        }
        #endregion
    }
}
