﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using Nito.AsyncEx;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Spotify;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Crypto;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Services.Mercury.Interfaces;

namespace SpotifyLibrary
{
    public class SpotifyConnectionResult
    {
        [CanBeNull] public readonly APLoginFailed ApFailed;
        [CanBeNull] public readonly APWelcome ApWelcome;
        public readonly TimeSpan Duration;

        public SpotifyConnectionResult(
            [CanBeNull] APWelcome apWelcome,
            [CanBeNull] APLoginFailed apFailed,
            TimeSpan duration)
        {
            ApWelcome = apWelcome;
            ApFailed = apFailed;
            Duration = duration;
        }
    }

    public class SpotifyConnection
    {
        private readonly AsyncLock _authLock = new();
        private readonly AsyncAutoResetEvent _authLockEventWaitHandle = new(false);

        private DiffieHellman _diffieHellman;
        private readonly IMercuryClient _mercuryClient;
        private readonly AsyncLock _recvLock = new();
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

        private readonly MercuryConnectionDisconnected connectionDisconnected;
        private readonly MercuryConnectionEstablished established;
        private SpotifyReceiver _receiver;
        private volatile Shannon _recvCipher = new();
        private volatile Shannon _sendCipher = new();
        private DateTime _startTime;

        private TcpClient _tcpClient;
        internal APLoginFailed ApFailed;

        internal APWelcome ApWelcome;

        private volatile int recvNonce;
        private volatile int sendNonce;

        private SpotifyClient _client;
        internal SpotifyConnection(SpotifyClient client,
            MercuryConnectionDisconnected connectionDisconnected,
            MercuryConnectionEstablished established, IMercuryClient mercuryClient)
        {
            _client = client;
            this.connectionDisconnected = connectionDisconnected;
            this.established = established;
            _mercuryClient = mercuryClient;
        }

        private NetworkStream NetworkStream => _tcpClient?.GetStream();

        public bool IsConnected
        {
            get
            {
                using (_authLock.Lock())
                {
                    try
                    {
                        return ApWelcome != null
                               && _tcpClient != null
                               && _tcpClient.Connected;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }

        public ConcurrentDictionary<string, string> UserAttributes { get; private set; }

        internal async Task<SpotifyConnectionResult> Connect()
        {
            if (IsConnected)
                return new SpotifyConnectionResult(ApWelcome, ApFailed, TimeSpan.Zero);

            _diffieHellman = new DiffieHellman();
            ResetNonce();
            ApWelcome = null;
            ApFailed = null;

            _startTime = DateTime.UtcNow;
            var sw = Stopwatch.StartNew();
            var tcpClientTask = await ApResolver.GetClosestAccessPoint();

            _tcpClient?.Dispose();
            _tcpClient = new TcpClient(tcpClientTask.host, tcpClientTask.port);

            var connected = ConnectToSpotify(NewClientHello(), _diffieHellman);

            if (!connected)
            {
                Debug.WriteLine("Couldn't connect... Retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5));
                return await Reconnect();
            }

            var authenticated =
                Authenticate(await _client.Config.Authenticator.Get(), _client.Config);
            if(authenticated.apWelcome == null && authenticated.failed == null)
            {
                Debug.WriteLine("Couldn't connect... Retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5));
                return await Reconnect();
            }
            ApWelcome = authenticated.apWelcome;
            ApFailed = authenticated.failed;
            _receiver?.Dispose();
            UserAttributes = new ConcurrentDictionary<string, string>();
            if (ApFailed == null)
            {
                _receiver = new SpotifyReceiver(this, _mercuryClient, UserAttributes); 
                established.Invoke(DateTime.UtcNow - _startTime);

            }

            return new SpotifyConnectionResult(authenticated.apWelcome, authenticated.failed, sw.Elapsed);
        }

        public string CountryCode => _receiver?.CountryCode;
        internal async Task<SpotifyConnectionResult> Reconnect()
        {
            connectionDisconnected.Invoke(_startTime, DateTime.UtcNow, ConnectionDroppedReason.Reconnect);
            try
            {
                if (_tcpClient != null) _tcpClient.Dispose();
                return await Connect();
            }
            catch (Exception x)
            {
                _tcpClient = null;
                Debug.WriteLine("Failed reconnecting, retrying in 10 seconds... " + x);

                await Task.Delay(TimeSpan.FromSeconds(10));
                return await Reconnect();
            }
        }

        private bool ConnectToSpotify(ClientHello clientHello, DiffieHellman keys)
        {
            try
            {
                NetworkStream.ReadTimeout = (int) TimeSpan.FromSeconds(5)
                    .TotalMilliseconds;
                var clientHelloBytes = clientHello.ToByteArray();

                var (accumulator, aPresponseBytes) = WriteAccumulator(clientHelloBytes);

                if (accumulator == null) return false;

                using var data = ReadApResponseMessage(accumulator, aPresponseBytes, keys);

                if (NetworkStream.DataAvailable)
                    try
                    {
                        var scrap = new byte[4];
                        NetworkStream.ReadTimeout = 300;
                        var read = NetworkStream.Read(scrap, 0, scrap.Length);
                        if (read == scrap.Length)
                        {
                            var length = (scrap[0] << 24) | (scrap[1] << 16) | (scrap[2] << 8) | (scrap[3] & 0xFF);
                            var payload = new byte[length - 4];
                            NetworkStream.ReadComplete(payload, 0, payload.Length);
                            var failed = APResponseMessage.Parser.ParseFrom(payload)?.LoginFailed;
                            throw new SpotifyAuthenticatedException(failed);
                        }

                        if (read > 0) throw new Exception("Read unknown data!");
                    }
                    catch (Exception x)
                    {
                        // ignored
                    }

                NetworkStream.ReadTimeout = Timeout.Infinite;


                _sendCipher = new Shannon();
                _sendCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x14, 0x34));

                _recvCipher = new Shannon();
                _recvCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x34, 0x54));
                _authLockEventWaitHandle.Set();
                accumulator.Dispose();
                return true;
            }
            catch(Exception x)
            {
                Debug.WriteLine(x.ToString());
                return false;
            }
        }

        private (APWelcome? apWelcome, APLoginFailed? failed) Authenticate(
            LoginCredentials credentials,
            SpotifyConfiguration config)
        {
            if (_recvCipher == null) throw new Exception("Not connected");
            if (_sendCipher == null) throw new Exception("Not Connected");

            var clientResponseEncrypted = GetNewEncrypted(credentials, config);

            SendUnchecked(MercuryPacketType.Login,
                clientResponseEncrypted.ToByteArray(), CancellationToken.None);

            var packet = Receive(CancellationToken.None, false);
            switch (packet.Cmd)
            {
                case MercuryPacketType.APWelcome:
                {
                    var apWelcome = APWelcome.Parser.ParseFrom(packet.Payload);

                    var bytes0X0F = new byte[20];
                    new Random().NextBytes(bytes0X0F);
                    SendUnchecked(MercuryPacketType.Unknown_0x0f,
                        bytes0X0F,
                        CancellationToken.None);

                    using var preferredLocale = new MemoryStream(18 + 5);
                    preferredLocale.WriteByte(0x0);
                    preferredLocale.WriteByte(0x0);
                    preferredLocale.WriteByte(0x10);
                    preferredLocale.WriteByte(0x0);
                    preferredLocale.WriteByte(0x02);
                    preferredLocale.Write("preferred-locale");
                    preferredLocale.Write(config.Locale);

                    try
                    {
                        if (config.StoreCredentials)
                        {
                            var jsonObj = new StoredCredentials
                            {
                                AuthenticationType = apWelcome.ReusableAuthCredentialsType,
                                Base64Credentials = apWelcome.ReusableAuthCredentials.ToBase64(),
                                Username = apWelcome.CanonicalUsername
                            };
                            config.StoreCredentialsFunction!(jsonObj);
                            //  ApplicationData.Current.LocalSettings.Values["auth_data"] =
                            // JsonConvert.SerializeObject(jsonObj);
                        }
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine(x.ToString());
                        throw;
                    }

                    return (apWelcome, null);
                }
                case MercuryPacketType.AuthFailure:
                    var apFailed = APLoginFailed.Parser.ParseFrom(packet.Payload);
                    return (null, apFailed);
                case MercuryPacketType.None:
                    return (null, null);
                    break;
                default:
                    throw new MercuryUnknownCmdException(packet);
            }
        }

        public MercuryPacket Receive(CancellationToken cts, bool withReconnect)
        {
            try
            {
                using (_recvLock.Lock(cts))
                {
                    _recvCipher.nonce(recvNonce.ToByteArray());
                    Interlocked.Increment(ref recvNonce);

                    var headerBytes = new byte[3];
                    NetworkStream.ReadComplete(headerBytes, 0, headerBytes.Length);
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
                    return new MercuryPacket((MercuryPacketType) cmd, payloadBytes);
                }
            }
            catch (IOException e)
            {
                if (withReconnect)
                {
                    _ = Reconnect();
                }

                return new MercuryPacket(MercuryPacketType.None, null);
            }
        }

        public void Send(MercuryPacketType cmd,
            byte[] payload,
            CancellationToken closedToken)
        {
            if (closedToken.IsCancellationRequested)
            {
                Debug.WriteLine("Connection was broken while Session.close() has been called");
                return;
            }

            SendUnchecked(cmd,
                payload,
                CancellationToken.None);
        }


        private void SendUnchecked(MercuryPacketType cmd, byte[] payload, CancellationToken cts)
        {
            using (_sendLock.Lock(cts))
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
                try
                {
                    NetworkStream.Write(bufferBytes, 0, bufferBytes.Length);
                    NetworkStream.Write(fourBytesBuffer, 0, fourBytesBuffer.Length);
                    NetworkStream.Flush();
                }
                catch (IOException x)
                {
                    _ = Reconnect();
                    //connectionDisconnected.Invoke(_startTime, DateTime.UtcNow, ConnectionDroppedReason.Internet);
                }
            }
        }

        private (MemoryStream Accumulator, byte[] APresponseBytes) WriteAccumulator(byte[] clientHelloBytes)
        {
            try
            {
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
                return (accumulator, tmp);
            }
            catch (IOException x)
            {
                _ = Reconnect();
                //connectionDisconnected.Invoke(_startTime, DateTime.UtcNow, ConnectionDroppedReason.Internet);
                return (null, new byte[0]);
            }
        }

        private MemoryStream ReadApResponseMessage(MemoryStream accumulator,
            byte[] aPresponseBytes,
            DiffieHellman keys)
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

            NetworkStream.WriteByte(0x00);
            NetworkStream.WriteByte(0x00);
            NetworkStream.WriteByte(0x00);
            var bytesb = BitConverter.GetBytes(len);
            NetworkStream.WriteByte(bytesb[0]);
            NetworkStream.Write(clientResponsePlaintextBytes, 0, clientResponsePlaintextBytes.Length);
            NetworkStream.Flush();
            return data;
        }

        private ClientResponseEncrypted GetNewEncrypted(
            LoginCredentials credentials,
            SpotifyConfiguration config)
        {
            return new()
            {
                LoginCredentials = credentials,
                SystemInfo = new SystemInfo
                {
                    Os = Os.Windows,
                    CpuFamily = CpuFamily.CpuX86,
                    SystemInformationString = "1",
                    DeviceId = config.DeviceId
                },
                VersionString = "1.0"
            };
        }

        private void ResetNonce()
        {
            sendNonce = 0;
            recvNonce = 0;
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
    }
}