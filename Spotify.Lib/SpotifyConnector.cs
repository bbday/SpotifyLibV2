#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Google.Protobuf;
using Nito.AsyncEx;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Spotify.Lib.Crypto;
using Spotify.Lib.Exceptions;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;

namespace Spotify.Lib
{
    internal class SpotifyConnector : PacketsManager
    {
        private static readonly int MercuryRequestTimeout = 3000;
        private readonly AsyncLock _authLock = new();

        private readonly ConcurrentDictionary<long, ICallback> _callbacks =
            new();

        private readonly ConcurrentDictionary<long, BytesArrayList> _partials
            = new();

        private readonly AsyncLock _recvLock = new();

        private readonly EventWaitHandle _removeCallbackLock =
            new AutoResetEvent(false);

        private readonly AsyncLock _sendLock = new();

        private readonly byte[] _serverKey =
        {
            0xac, 0xe0, 0x46, 0x0b, 0xff, 0xc2, 0x30, 0xaf,
            0xf4, 0x6b, 0xfe, 0xc3,
            0xbf, 0xbf, 0x86, 0x3d, 0xa1, 0x91, 0xc6, 0xcc,
            0x33, 0x6c, 0x93, 0xa1,
            0x4f, 0xb3, 0xb0, 0x16, 0x12, 0xac, 0xac, 0x6a,
            0xf1, 0x80, 0xe7, 0xf6,
            0x14, 0xd9, 0x42, 0x9d, 0xbe, 0x2e, 0x34, 0x66,
            0x43, 0xe3, 0x62, 0xd2,
            0x32, 0x7a, 0x1a, 0x0d, 0x92, 0x3b, 0xae, 0xdd,
            0x14, 0x02, 0xb1, 0x81,
            0x55, 0x05, 0x61, 0x04, 0xd5, 0x2c, 0x96, 0xa4,
            0x4c, 0x1e, 0xcc, 0x02,
            0x4a, 0xd4, 0xb2, 0x0c, 0x00, 0x1f, 0x17, 0xed,
            0xc2, 0x2f, 0xc4, 0x35,
            0x21, 0xc8, 0xf0, 0xcb, 0xae, 0xd2, 0xad, 0xd7,
            0x2b, 0x0f, 0x9d, 0xb3,
            0xc5, 0x32, 0x1a, 0x2a, 0xfe, 0x59, 0xf3, 0x5a,
            0x0d, 0xac, 0x68, 0xf1,
            0xfa, 0x62, 0x1e, 0xfb, 0x2c, 0x8d, 0x0c, 0xb7,
            0x39, 0x2d, 0x92, 0x47,
            0xe3, 0xd7, 0x35, 0x1a, 0x6d, 0xbd, 0x24, 0xc2,
            0xae, 0x25, 0x5b, 0x88,
            0xff, 0xab, 0x73, 0x29, 0x8a, 0x0b, 0xcc, 0xcd,
            0x0c, 0x58, 0x67, 0x31,
            0x89, 0xe8, 0xbd, 0x34, 0x80, 0x78, 0x4a, 0x5f,
            0xc9, 0x6b, 0x89, 0x9d,
            0x95, 0x6b, 0xfc, 0x86, 0xd7, 0x4f, 0x33, 0xa6,
            0x78, 0x17, 0x96, 0xc9,
            0xc3, 0x2d, 0x0d, 0x32, 0xa5, 0xab, 0xcd, 0x05,
            0x27, 0xe2, 0xf7, 0x10,
            0xa3, 0x96, 0x13, 0xc4, 0x2f, 0x99, 0xc0, 0x27,
            0xbf, 0xed, 0x04, 0x9c,
            0x3c, 0x27, 0x58, 0x04, 0xb6, 0xb2, 0x19, 0xf9,
            0xc1, 0x2f, 0x02, 0xe9,
            0x48, 0x63, 0xec, 0xa1, 0xb6, 0x42, 0xa0, 0x9d,
            0x48, 0x25, 0xf8, 0xb3,
            0x9d, 0xd0, 0xe8, 0x6a, 0xf9, 0x48, 0x4d, 0xa1,
            0xc2, 0xba, 0x86, 0x30,
            0x42, 0xea, 0x9d, 0xb3, 0x08, 0x6c, 0x19, 0x0e,
            0x48, 0xb3, 0x9d, 0x66,
            0xeb, 0x00, 0x06, 0xa2, 0x5a, 0xee, 0xa1, 0x1b,
            0x13, 0x87, 0x3c, 0xd7,
            0x19, 0xe6, 0x55, 0xbd
        };

        private readonly List<InternalSubListener> _subscriptions =
            new();

        private readonly AsyncLock handleLock = new();

        // private TcpClient? _tcpClient;
        private DiffieHellman _diffieHellman;
        private volatile Shannon _recvCipher = new();
        private volatile Shannon _sendCipher = new();
        private volatile int _seqHolder;


        private TcpClient? _tcpClient;
        private volatile int recvNonce;
        private volatile int sendNonce;

        protected SpotifyConnector() : base("mercury-pm")
        {
        }

        internal APWelcome ApWelcome { get; private set; }

        public bool IsAuthenticated
        {
            get
            {
                try
                {
                    return ApWelcome != null
                           && (_tcpClient?.Connected ?? false);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private NetworkStream? NetworkStream
        {
            get
            {
                var nullableBool = _tcpClient?.Connected;
                if (!nullableBool.HasValue || !nullableBool.Value) return null;
                return _tcpClient?.GetStream();
            }
        }

        public static SpotifyConnector Instance { get; private set; }
        internal CancellationTokenSource MainCancellationToken { get; private set; }


        public static async Task<SpotifyConnector> Connect(IAuthenticator authenticator)
        {
            if (Instance is {IsAuthenticated: true})
                return Instance;

            var newConnection =
                new SpotifyConnector();

            await newConnection.ConnectInternal();

            var authenticated = await
                newConnection.Authenticate(await authenticator.Get());

            return newConnection;
        }

        private async Task ConnectInternal()
        {
            var retries = 0;
            while (retries < 7)
            {
                retries++;

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                           SecurityProtocolType.Tls12;

                    using (await _authLock.LockAsync())
                    {
                        _diffieHellman = new DiffieHellman();
                        var (host, port) = await ApResolver.GetClosestAccessPoint();

                        //Dispose old client and initialize new tcp client
                        //_tcpClient?.Dispose();
                        //_tcpClient = new TcpClient(host, port);
                        Debug.WriteLine($"Selected: {host} : {port}");
                        _tcpClient?.Dispose();
                        _tcpClient = new TcpClient(host, port);

                        var clientHello = NewClientHello();
                        var clientHelloBytes = clientHello.ToByteArray();

                        //Write the initial client hello..
                        NetworkStream.ReadTimeout = 100;
                        NetworkStream.WriteByte(0x00);
                        NetworkStream.WriteByte(0x04);
                        NetworkStream.WriteByte(0x00);
                        NetworkStream.WriteByte(0x00);
                        NetworkStream.WriteByte(0x00);
                        NetworkStream.Flush();
                        var length = 2 + 4 + clientHelloBytes.Length;
                        var bytes = BitConverter.GetBytes(length);

                        NetworkStream.WriteByte(bytes[0]);
                        NetworkStream.Write(clientHelloBytes, 0, clientHelloBytes.Length);
                        NetworkStream.Flush();

                        var buffer = new byte[1000];


                        var len = int.Parse(NetworkStream.Read(buffer, 0, buffer.Length).ToString());
                        var tmp = new byte[len];
                        Array.Copy(buffer, tmp, len);

                        tmp = tmp.Skip(4).ToArray();
                        var accumulator = new MemoryStream();
                        accumulator.WriteByte(0x00);
                        accumulator.WriteByte(0x04);

                        var lnarr = length.ToByteArray();
                        accumulator.Write(lnarr, 0, lnarr.Length);
                        accumulator.Write(clientHelloBytes, 0, clientHelloBytes.Length);

                        var lenArr = len.ToByteArray();
                        accumulator.Write(lenArr, 0, lenArr.Length);
                        accumulator.Write(tmp, 0, tmp.Length);

                        using var data = ReadApResponseMessage(accumulator, tmp, _diffieHellman, NetworkStream);

                        if (NetworkStream.DataAvailable)
                            //if data is available, it could be scrap or a failed login.
                            try
                            {
                                var scrap = new byte[4];
                                NetworkStream.ReadTimeout = 300;
                                var read = NetworkStream.Read(scrap, 0, scrap.Length);
                                if (read == scrap.Length)
                                {
                                    var lengthOfScrap = (scrap[0] << 24) | (scrap[1] << 16) | (scrap[2] << 8) |
                                                        (scrap[3] & 0xFF);
                                    var payload = new byte[length - 4];
                                    NetworkStream.ReadComplete(payload, 0, payload.Length);
                                    var failed = APResponseMessage.Parser.ParseFrom(payload);
                                    throw new SpotifyConnectionException(failed);
                                }

                                if (read > 0) throw new UnknownDataException(scrap);
                            }
                            catch (Exception x)
                            {
                                // ignored
                            }

                        //Reset network timeout to infinite. This will allow use to wait for messages.
                        NetworkStream.ReadTimeout = Timeout.Infinite;

                        _sendCipher = new Shannon();
                        _sendCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x14, 0x34));

                        _recvCipher = new Shannon();
                        _recvCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x34, 0x54));
                        accumulator.Dispose();
                        return;
                    }
                }
                catch (IOException x)
                {
                    SpotifyClient.Instance.OnConnectionDropped(ConnectionDroppedReason.Retry);
                    Debug.WriteLine(x.ToString());
                    if (retries == 6) throw;
                }
            }

            SpotifyClient.Instance.OnConnectionDropped(ConnectionDroppedReason.Unknown);
        }

        private MemoryStream ReadApResponseMessage(MemoryStream accumulator,
            byte[] aPresponseBytes,
            DiffieHellman keys,
            NetworkStream networkStream)
        {
            var apResponseMessage = APResponseMessage.Parser.ParseFrom(aPresponseBytes);
            var sharedKey = ByteExtensions.ToByteArray(keys.ComputeSharedKey(apResponseMessage
                .Challenge.LoginCryptoChallenge.DiffieHellman.Gs.ToByteArray()));

            // Check gs_signature
            var rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = new RSAParameters
            {
                Modulus = new BigInteger(1, _serverKey).ToByteArrayUnsigned(),
                Exponent = BigInteger.ValueOf(65537).ToByteArrayUnsigned()
            };

            //Import key parameters into RSA.
            rsa.ImportParameters(rsaKeyInfo);
            var gs = apResponseMessage.Challenge.LoginCryptoChallenge.DiffieHellman.Gs.ToByteArray();
            var sign = apResponseMessage.Challenge.LoginCryptoChallenge.DiffieHellman.GsSignature.ToByteArray();

            if (!rsa.VerifyData(gs,
                sign,
                HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1))
                throw new AccessViolationException("Failed to verify APResponse");

            // Solve challenge
            var binaryData = accumulator.ToArray();
            using var data = new MemoryStream();
            var mac = new HMACSHA1(sharedKey);
            mac.Initialize();
            for (var i = 1; i < 6; i++)
            {
                mac.TransformBlock(binaryData, 0, binaryData.Length, null, 0);
                var temp = new[] {(byte) i};
                mac.TransformBlock(temp, 0, temp.Length, null, 0);
                mac.TransformFinalBlock(new byte[0], 0, 0);
                var final = mac.Hash;
                data.Write(final, 0, final.Length);
                mac = new HMACSHA1(sharedKey);
            }

            var dataArray = data.ToArray();
            mac = new HMACSHA1(Arrays.CopyOfRange(dataArray, 0, 0x14));
            mac.TransformBlock(binaryData, 0, binaryData.Length, null, 0);
            mac.TransformFinalBlock(new byte[0], 0, 0);
            var challenge = mac.Hash;
            var clientResponsePlaintext = new ClientResponsePlaintext
            {
                LoginCryptoResponse = new LoginCryptoResponseUnion
                {
                    DiffieHellman = new LoginCryptoDiffieHellmanResponse
                    {
                        Hmac = ByteString.CopyFrom(challenge)
                    }
                },
                PowResponse = new PoWResponseUnion(),
                CryptoResponse = new CryptoResponseUnion()
            };
            var clientResponsePlaintextBytes = clientResponsePlaintext.ToByteArray();
            var len = 4 + clientResponsePlaintextBytes.Length;

            networkStream.WriteByte(0x00);
            networkStream.WriteByte(0x00);
            networkStream.WriteByte(0x00);
            var bytesb = BitConverter.GetBytes(len);
            networkStream.WriteByte(bytesb[0]);
            networkStream.Write(clientResponsePlaintextBytes, 0, clientResponsePlaintextBytes.Length);
            networkStream.Flush();
            return data;
        }

        private async Task<APWelcome> Authenticate(
            LoginCredentials credentials)
        {
            if (_recvCipher == null) throw new Exception("Not connected");
            if (_sendCipher == null) throw new Exception("Not Connected");

            var clientResponseEncrypted = GetNewEncrypted(credentials);

            await SendPackage(MercuryPacketType.Login,
                clientResponseEncrypted.ToByteArray());

            var packet = await ReceivePackage(CancellationToken.None);
            switch (packet.Cmd)
            {
                case MercuryPacketType.APWelcome:
                {
                    var apWelcome = APWelcome.Parser.ParseFrom(packet.Payload);
                    //update locale
                    var bytes0X0F = new byte[20];
                    new Random().NextBytes(bytes0X0F);
                    await SendPackage(MercuryPacketType.Unknown_0x0f,
                        bytes0X0F,
                        CancellationToken.None);

                    using var preferredLocale = new MemoryStream(18 + 5);
                    preferredLocale.WriteByte(0x0);
                    preferredLocale.WriteByte(0x0);
                    preferredLocale.WriteByte(0x10);
                    preferredLocale.WriteByte(0x0);
                    preferredLocale.WriteByte(0x02);
                    preferredLocale.Write("preferred-locale");
                    preferredLocale.Write(SpotifyConfig.Locale);
                    await SendPackage(MercuryPacketType.PreferredLocale, preferredLocale.ToArray());
                    ApWelcome = apWelcome;

                    MainCancellationToken?.Cancel();
                    MainCancellationToken?.Dispose();
                    MainCancellationToken = new CancellationTokenSource();
                    SpotifyClient.Instance.OnConnectionInstantiated(new StoredCredentials
                    {
                        AuthenticationType = apWelcome.ReusableAuthCredentialsType,
                        Base64Credentials = apWelcome.ReusableAuthCredentials.ToBase64(),
                        Username = apWelcome.CanonicalUsername
                    });
                    var ts = new ThreadStart(BackgroundMethod);
                    var backgroundThread = new Thread(ts);
                    backgroundThread.Start();
                    return apWelcome;
                }
                case MercuryPacketType.AuthFailure:
                    throw new SpotifyAuthenticatedException(APLoginFailed.Parser.ParseFrom(packet.Payload));
                default:
                    throw new SpotifyConnectionException(null);
            }

            //throw new InvalidOperationException();
        }

        internal async Task<int> SendRequest(RawMercuryRequest request, SyncCallback callback, CancellationToken ct)
        {
            using (await handleLock.LockAsync(ct))
            {
                var requestPayload = request._payload.ToArray();
                var requestHeader = request._header;
                if (requestPayload == null || requestHeader == null)
                    throw new Exception("An unknown error occured. the librar could be outdated");

                var bytesOut = new MemoryStream();
                var s4B = BitConverter.GetBytes((short) 4).Reverse().ToArray();
                bytesOut.Write(s4B, 0, s4B.Length); // Seq length


                var seqB = BitConverter.GetBytes(Interlocked.Increment(ref _seqHolder)).Reverse().ToArray();
                bytesOut.Write(seqB, 0, seqB.Length); // Seq

                bytesOut.WriteByte(1); // Flags
                var reqpB = BitConverter.GetBytes((short) (1 + requestPayload.Length)).Reverse().ToArray();
                bytesOut.Write(reqpB, 0, reqpB.Length); // Parts count

                var headerBytes2 = requestHeader.ToByteArray();
                var hedBls = BitConverter.GetBytes((short) headerBytes2.Length).Reverse().ToArray();

                bytesOut.Write(hedBls, 0, hedBls.Length); // Header length
                bytesOut.Write(headerBytes2, 0, headerBytes2.Length); // Header

                foreach (var part in requestPayload)
                {
                    // Parts
                    var l = BitConverter.GetBytes((short) part.Length).Reverse().ToArray();
                    bytesOut.Write(l, 0, l.Length);
                    bytesOut.Write(part, 0, part.Length);
                }

                var cmd = request._header.Method.ToLower() switch
                {
                    "sub" => MercuryPacketType.MercurySub,
                    "unsub" => MercuryPacketType.MercuryUnsub,
                    _ => MercuryPacketType.MercuryReq
                };

                _callbacks.TryAdd(_seqHolder, callback);
                await SendPackage(cmd, bytesOut.ToArray(), ct);
                return _seqHolder;
            }
        }

        internal async Task SendPackage(
            MercuryPacketType cmd,
            byte[] payload,
            CancellationToken? sendToken = null)
        {
            if (sendToken == null)
            {
                var cts = new CancellationTokenSource();
                //cts.CancelAfter(TimeSpan.FromSeconds(5));
                sendToken = cts.Token;
            }

            var retryCount = 0;
            while (retryCount < 3)
            {
                retryCount++;
                try
                {
                    using (_sendLock.Lock(sendToken.Value))
                    {
                        var payloadLengthAsByte = BitConverter.GetBytes((short) payload.Length).Reverse().ToArray();
                        using var yetAnotherBuffer = new MemoryStream(3 + payload.Length);
                        yetAnotherBuffer.WriteByte((byte) cmd);
                        yetAnotherBuffer.Write(payloadLengthAsByte, 0, payloadLengthAsByte.Length);
                        yetAnotherBuffer.Write(payload, 0, payload.Length);

                        _sendCipher.nonce(sendNonce.ToByteArray());
                        Interlocked.Increment(ref sendNonce);

                        var bufferBytes = yetAnotherBuffer.ToArray();
                        _sendCipher.encrypt(bufferBytes);

                        var fourBytesBuffer = new byte[4];
                        _sendCipher.finish(fourBytesBuffer);

                        NetworkStream.Write(bufferBytes, 0, bufferBytes.Length);
                        NetworkStream.Write(fourBytesBuffer, 0, fourBytesBuffer.Length);
                        NetworkStream.Flush();
                        return;
                    }
                }
                catch (Exception x)
                {
                    Debug.WriteLine($"Reconnecting... {x}");
                    SpotifyClient.Instance.OnConnectionDropped(ConnectionDroppedReason.Retry);
                    await Reconnect(x);
                }
            }

            throw new MercuryCannotSendException("Error unknown..");
        }

        public async Task<MercuryPacket> ReceivePackage(CancellationToken? sendToken = null)
        {
            CancellationTokenSource cts = null;
            if (sendToken == null)
            {
                cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                sendToken = cts.Token;
            }

            var retryCount = 0;
            while (retryCount < 3)
            {
                retryCount++;
                try
                {
                    using (_recvLock.Lock(sendToken.Value))
                    {
                        _recvCipher.nonce(recvNonce.ToByteArray());
                        Interlocked.Increment(ref recvNonce);

                        var headerBytes = new byte[3];
                        if (NetworkStream == null)
                            return new MercuryPacket(MercuryPacketType.UnknownData_AllZeros, new byte[0]);

                        NetworkStream.ReadComplete(headerBytes, 0,
                            headerBytes.Length);
                        _recvCipher.decrypt(headerBytes);

                        var cmd = headerBytes[0];
                        var payloadLength = (short) ((headerBytes[1] << 8) | (headerBytes[2] & 0xFF));

                        var payloadBytes = new byte[payloadLength];
                        NetworkStream.ReadComplete(payloadBytes, 0, payloadBytes.Length);
                        _recvCipher.decrypt(payloadBytes);

                        var mac = new byte[4];
                        NetworkStream.ReadComplete(mac, 0, mac.Length);

                        var expectedMac = new byte[4];
                        _recvCipher.finish(expectedMac);
                        cts?.Dispose();
                        return new MercuryPacket((MercuryPacketType) cmd, payloadBytes);
                    }
                }
                catch (Exception x)
                {
                    Debug.WriteLine($"Reconnecting... {x}");
                    await Reconnect(x);
                }
            }

            cts?.Dispose();
            throw new MercuryCannotReceiveException("Unknown");
        }

        private async Task Reconnect(Exception ioException)
        {
            //throw new NotImplementedException();
        }

        private ClientResponseEncrypted GetNewEncrypted(
            LoginCredentials credentials)
        {
            return new()
            {
                LoginCredentials = credentials,
                SystemInfo = new SystemInfo
                {
                    Os = Os.Windows,
                    CpuFamily = CpuFamily.CpuX86,
                    SystemInformationString = "1",
                    DeviceId = SpotifyConfig.DeviceId
                },
                VersionString = "1.0"
            };
        }

        private ClientHello NewClientHello()
        {
            var clientHello = new ClientHello
            {
                BuildInfo = new BuildInfo
                {
                    Platform = Platform.Win32X86,
                    Product = Product.Client,
                    ProductFlags = {ProductFlags.ProductFlagNone},
                    Version = 112800721
                }
            };
            clientHello.CryptosuitesSupported.Add(Cryptosuite.Shannon);
            clientHello.LoginCryptoHello = new LoginCryptoHelloUnion
            {
                DiffieHellman = new LoginCryptoDiffieHellmanHello
                {
                    Gc = ByteString.CopyFrom(_diffieHellman.PublicKeyArray()),
                    ServerKeysKnown = 1
                }
            };
            var nonce = new byte[16];
            new Random().NextBytes(nonce);
            clientHello.ClientNonce = ByteString.CopyFrom(nonce);
            clientHello.Padding = ByteString.CopyFrom(30);

            return clientHello;
        }

        protected override void Handle(MercuryPacket packet)
        {
            using (handleLock.Lock())
            {
                using var stream = new MemoryStream(packet.Payload);
                int seqLength = getShort(packet.Payload, (int) stream.Position, true);
                stream.Seek(2, SeekOrigin.Current);
                long seq = 0;
                var buffer = packet.Payload;
                switch (seqLength)
                {
                    case 2:
                        seq = getShort(packet.Payload, (int) stream.Position, true);
                        stream.Seek(2, SeekOrigin.Current);
                        break;
                    case 4:
                        seq = getInt(packet.Payload, (int) stream.Position, true);
                        stream.Seek(4, SeekOrigin.Current);
                        break;
                    case 8:
                        seq = getLong(packet.Payload, (int) stream.Position, true);
                        stream.Seek(8, SeekOrigin.Current);
                        break;
                }

                var flags = packet.Payload[(int) stream.Position];
                stream.Seek(1, SeekOrigin.Current);
                var parts = getShort(packet.Payload, (int) stream.Position, true);
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
                    var size = getShort(packet.Payload, (int) stream.Position, true);
                    stream.Seek(2, SeekOrigin.Current);

                    var buffer2 = new byte[size];

                    var end = buffer2.Length;
                    for (var z = 0; z < end; z++)
                    {
                        var a = packet.Payload[(int) stream.Position];
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
        }

        protected override void Exception(Exception ex)
        {
            throw new NotImplementedException();
        }

        private async void BackgroundMethod()
        {
            Debug.WriteLine("Session.Receiver started");

            while (!MainCancellationToken.IsCancellationRequested)
                try
                {
                    var packet = await ReceivePackage(MainCancellationToken.Token);
                    if (!Enum.TryParse(packet.Cmd.ToString(), out MercuryPacketType cmd))
                    {
                        Debug.WriteLine(
                            $"Skipping unknown command cmd: {packet.Cmd}, payload: {packet.Payload.BytesToHex()}");
                        continue;
                    }

                    switch (cmd)
                    {
                        case MercuryPacketType.Ping:
                            try
                            {
                                await SendPackage(MercuryPacketType.Pong, packet.Payload, MainCancellationToken.Token);
                            }
                            catch (IOException ex)
                            {
                                Debug.WriteLine("Failed sending Pong!", ex);
                            }

                            break;
                        case MercuryPacketType.PongAck:
                            break;
                        case MercuryPacketType.CountryCode:
                            var countryCode = Encoding.Default.GetString(packet.Payload);
                            SpotifyClient.Instance.Country = countryCode;
                            Debug.WriteLine("Received CountryCode: " + countryCode);
                            break;
                        case MercuryPacketType.LicenseVersion:
                            Debug.WriteLine($"Received LicenseVersion: {Encoding.Default.GetString(packet.Payload)}");
                            using (var m = new MemoryStream(packet.Payload))
                            {
                                var id = m.GetShort();
                                if (id != 0)
                                {
                                    //var buffer = new byte[m.Get()];
                                    //   m.Get(buffer);
                                    // Debug.WriteLine(
                                    // $"Received LicenseVersion: {id}, {Encoding.Default.GetString(buffer)}");
                                }
                                else
                                {
                                    Debug.WriteLine($"Received LicenseVersion: {id}");
                                }
                            }

                            break;
                        case MercuryPacketType.MercuryReq:
                        case MercuryPacketType.MercurySub:
                        case MercuryPacketType.MercuryUnsub:
                        case MercuryPacketType.MercuryEvent:
                            Dispatch(packet);
                            break;
                        case MercuryPacketType.AesKey:
                        case MercuryPacketType.AesKeyError:
                             SpotifyClient.Instance.KeyManager.Dispatch(packet);
                            break;
                        case MercuryPacketType.ProductInfo:
                            try
                            {
                                Debug.Write("Received product info.");
                                ParseProductInfo(packet.Payload);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Failed parsing prodcut info!" + ex);
                            }

                            break;

                        case MercuryPacketType.Unknown_0x10:
                            Debug.WriteLine("Received 0x10 : " + packet.Payload.BytesToHex());
                            break;
                    }
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                }
        }

        private void ParseProductInfo(byte[] @in)
        {
            var productInfoString = Encoding.Default.GetString(@in);
            Debug.WriteLine(productInfoString);
            var xml = new XmlDocument();
            xml.LoadXml(productInfoString);

            var products = xml.SelectNodes("products");
            if (products != null && products.Count > 0)
            {
                var firstItemAsProducts = products[0];

                var product = firstItemAsProducts.ChildNodes[0];

                var properties = product.ChildNodes;
                for (var i = 0; i < properties.Count; i++)
                {
                    var node = properties.Item(i);
                    SpotifyClient.Instance.UserAttributes.AddOrUpdate(node.Name, node.InnerText);
                }
            }
        }

        #region PRivates

        private static short getShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short) (!obj2 ? (int) getShortL(obj0, obj1) : (int) getShortB(obj0, obj1));
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
            return (short) (((sbyte) obj0 << 8) | ((sbyte) obj1 & byte.MaxValue));
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
            return ((sbyte) obj0 << 24) | (((sbyte) obj1 & byte.MaxValue) << 16) |
                   (((sbyte) obj2 & byte.MaxValue) << 8) | ((sbyte) obj3 & byte.MaxValue);
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
            return ((long) (sbyte) obj0 << 56)
                   | (((sbyte) obj1 & (long) byte.MaxValue) << 48)
                   | (((sbyte) obj2 & (long) byte.MaxValue) << 40)
                   | (((sbyte) obj3 & (long) byte.MaxValue) << 32)
                   | (((sbyte) obj4 & (long) byte.MaxValue) << 24)
                   | (((sbyte) obj5 & (long) byte.MaxValue) << 16)
                   | (((sbyte) obj6 & (long) byte.MaxValue) << 8)
                   | ((sbyte) obj7 & (long) byte.MaxValue);
        }

        #endregion
    }
}