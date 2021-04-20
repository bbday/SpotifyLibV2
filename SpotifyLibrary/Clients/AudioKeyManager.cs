using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Google.Protobuf;
using SpotifyLibrary.Bases;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Clients
{
    public class AudioKeyManager : PacketsManager, IAudioKeyManager
    {
        private volatile int seqHolder;
        private static readonly byte[] ZERO_SHORT = new byte[] { 0, 0 };
        private readonly SpotifyLibrary _session;

        private readonly ConcurrentDictionary<int, IKeyCallBack> _callbacks =
            new ConcurrentDictionary<int, IKeyCallBack>();

        public AudioKeyManager
        (SpotifyLibrary session)
            : base("audio-keys")
        {
            _session = session;
            seqHolder = 0;
        }

        public byte[] GetAudioKey(
             ByteString gid,
             ByteString fileId,
            bool retry = true)
        {
            Interlocked.Increment(ref seqHolder);
            using var @out = new MemoryStream();
            fileId.WriteTo(@out);
            gid.WriteTo(@out);
            var b = seqHolder.ToByteArray();
            @out.Write(b, 0, b.Length);
            @out.Write(ZERO_SHORT, 0, ZERO_SHORT.Length);
            _session.MercuryClient.Connection.Send(MercuryPacketType.RequestKey, @out.ToArray(), CancellationToken.None);

            using var callback = new KeyCallBack();
            _callbacks.TryAdd(seqHolder, callback);

            var key = callback.WaitResponse();
            if (key != null) return key;
            if (retry) return GetAudioKey(gid, fileId, false);
            throw new AesKeyException(
                $"Failed fetching audio key! gid: " +
                $"{gid.ToByteArray().BytesToHex()}," +
                $" fileId: {fileId.ToByteArray().BytesToHex()}");
        }

        protected override void Handle(MercuryPacket packet)
        {
            using var payload = new MemoryStream(packet.Payload);
            var seq = 0;
            var buffer = packet.Payload;
            seq = getInt(packet.Payload, (int)payload.Position, true);
            payload.Seek(4, SeekOrigin.Current);
            _callbacks.TryRemove(seq, out var callback);
            if (callback == null)
            {
                Debug.WriteLine("Couldn't find callback for seq: " + seq);
                return;
            }

            switch (packet.Cmd)
            {
                case MercuryPacketType.AesKey:
                    var key = new byte[16];
                    payload.Read(key, 0, key.Length);
                    callback.Key(key);
                    break;
                case MercuryPacketType.AesKeyError:
                    var code = getShort(packet.Payload, (int)payload.Position, true);
                    payload.Seek(2, SeekOrigin.Current);
                    callback.Error(code);
                    break;
                default:
                    Debug.WriteLine("Couldn't handle packet, cmd: {0}, length: {1}", packet.Cmd, packet.Payload.Length);
                    break;
            }
        }

        protected override void Exception(Exception ex)
        {
            throw new NotImplementedException();
        }


        private static int getInt(byte[] obj0, int obj1, bool obj2) =>
            !obj2 ? getIntL(obj0, obj1) : getIntB(obj0, obj1);

        private static int getIntB(byte[] obj0, int obj1) =>
            makeInt(obj0[obj1], obj0[obj1 + 1], obj0[obj1 + 2], obj0[obj1 + 3]);

        private static int getIntL(byte[] obj0, int obj1) =>
            makeInt(obj0[obj1 + 3], obj0[obj1 + 2], obj0[obj1 + 1], obj0[obj1]);

        private static int makeInt(byte obj0, byte obj1, byte obj2, byte obj3) => (int)(sbyte)obj0 << 24 |
            ((int)(sbyte)obj1 & (int)byte.MaxValue) << 16 | ((int)(sbyte)obj2 & (int)byte.MaxValue) << 8 |
            (int)(sbyte)obj3 & (int)byte.MaxValue;


        private static short getShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short)(!obj2 ? (int)getShortL(obj0, obj1) : (int)getShortB(obj0, obj1));
        }

        private static short getShortB(byte[] obj0, int obj1) => makeShort(obj0[obj1], obj0[obj1 + 1]);

        private static short getShortL(byte[] obj0, int obj1)
        {
            return makeShort(obj0[obj1 + 1], obj0[obj1]);
        }

        private static short makeShort(byte obj0, byte obj1) =>
            (short)((int)(sbyte)obj0 << 8 | (int)(sbyte)obj1 & (int)byte.MaxValue);


    }
}
