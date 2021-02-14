using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using Google.Protobuf;
using Nito.AsyncEx;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Spotify;
using SpotifyLibV2.Config;
using SpotifyLibV2.Crypto;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2
{
    public class SpotifyStream : NetworkStream, ISpotifyStream
    {
        private readonly Nito.AsyncEx.AsyncAutoResetEvent _authLockEventWaitHandle = new AsyncAutoResetEvent(false);
        private readonly Nito.AsyncEx.AsyncLock _authLock = new AsyncLock();

        private readonly Nito.AsyncEx.AsyncLock _recvLock = new AsyncLock();
        private readonly Nito.AsyncEx.AsyncLock _sendLock = new AsyncLock();

        private readonly byte[] _serverKey =
       {
            (byte) 0xac, (byte) 0xe0, (byte) 0x46, (byte) 0x0b, (byte) 0xff, (byte) 0xc2, (byte) 0x30, (byte) 0xaf,
            (byte) 0xf4, (byte) 0x6b, (byte) 0xfe, (byte) 0xc3,
            (byte) 0xbf, (byte) 0xbf, (byte) 0x86, (byte) 0x3d, (byte) 0xa1, (byte) 0x91, (byte) 0xc6, (byte) 0xcc,
            (byte) 0x33, (byte) 0x6c, (byte) 0x93, (byte) 0xa1,
            (byte) 0x4f, (byte) 0xb3, (byte) 0xb0, (byte) 0x16, (byte) 0x12, (byte) 0xac, (byte) 0xac, (byte) 0x6a,
            (byte) 0xf1, (byte) 0x80, (byte) 0xe7, (byte) 0xf6,
            (byte) 0x14, (byte) 0xd9, (byte) 0x42, (byte) 0x9d, (byte) 0xbe, (byte) 0x2e, (byte) 0x34, (byte) 0x66,
            (byte) 0x43, (byte) 0xe3, (byte) 0x62, (byte) 0xd2,
            (byte) 0x32, (byte) 0x7a, (byte) 0x1a, (byte) 0x0d, (byte) 0x92, (byte) 0x3b, (byte) 0xae, (byte) 0xdd,
            (byte) 0x14, (byte) 0x02, (byte) 0xb1, (byte) 0x81,
            (byte) 0x55, (byte) 0x05, (byte) 0x61, (byte) 0x04, (byte) 0xd5, (byte) 0x2c, (byte) 0x96, (byte) 0xa4,
            (byte) 0x4c, (byte) 0x1e, (byte) 0xcc, (byte) 0x02,
            (byte) 0x4a, (byte) 0xd4, (byte) 0xb2, (byte) 0x0c, (byte) 0x00, (byte) 0x1f, (byte) 0x17, (byte) 0xed,
            (byte) 0xc2, (byte) 0x2f, (byte) 0xc4, (byte) 0x35,
            (byte) 0x21, (byte) 0xc8, (byte) 0xf0, (byte) 0xcb, (byte) 0xae, (byte) 0xd2, (byte) 0xad, (byte) 0xd7,
            (byte) 0x2b, (byte) 0x0f, (byte) 0x9d, (byte) 0xb3,
            (byte) 0xc5, (byte) 0x32, (byte) 0x1a, (byte) 0x2a, (byte) 0xfe, (byte) 0x59, (byte) 0xf3, (byte) 0x5a,
            (byte) 0x0d, (byte) 0xac, (byte) 0x68, (byte) 0xf1,
            (byte) 0xfa, (byte) 0x62, (byte) 0x1e, (byte) 0xfb, (byte) 0x2c, (byte) 0x8d, (byte) 0x0c, (byte) 0xb7,
            (byte) 0x39, (byte) 0x2d, (byte) 0x92, (byte) 0x47,
            (byte) 0xe3, (byte) 0xd7, (byte) 0x35, (byte) 0x1a, (byte) 0x6d, (byte) 0xbd, (byte) 0x24, (byte) 0xc2,
            (byte) 0xae, (byte) 0x25, (byte) 0x5b, (byte) 0x88,
            (byte) 0xff, (byte) 0xab, (byte) 0x73, (byte) 0x29, (byte) 0x8a, (byte) 0x0b, (byte) 0xcc, (byte) 0xcd,
            (byte) 0x0c, (byte) 0x58, (byte) 0x67, (byte) 0x31,
            (byte) 0x89, (byte) 0xe8, (byte) 0xbd, (byte) 0x34, (byte) 0x80, (byte) 0x78, (byte) 0x4a, (byte) 0x5f,
            (byte) 0xc9, (byte) 0x6b, (byte) 0x89, (byte) 0x9d,
            (byte) 0x95, (byte) 0x6b, (byte) 0xfc, (byte) 0x86, (byte) 0xd7, (byte) 0x4f, (byte) 0x33, (byte) 0xa6,
            (byte) 0x78, (byte) 0x17, (byte) 0x96, (byte) 0xc9,
            (byte) 0xc3, (byte) 0x2d, (byte) 0x0d, (byte) 0x32, (byte) 0xa5, (byte) 0xab, (byte) 0xcd, (byte) 0x05,
            (byte) 0x27, (byte) 0xe2, (byte) 0xf7, (byte) 0x10,
            (byte) 0xa3, (byte) 0x96, (byte) 0x13, (byte) 0xc4, (byte) 0x2f, (byte) 0x99, (byte) 0xc0, (byte) 0x27,
            (byte) 0xbf, (byte) 0xed, (byte) 0x04, (byte) 0x9c,
            (byte) 0x3c, (byte) 0x27, (byte) 0x58, (byte) 0x04, (byte) 0xb6, (byte) 0xb2, (byte) 0x19, (byte) 0xf9,
            (byte) 0xc1, (byte) 0x2f, (byte) 0x02, (byte) 0xe9,
            (byte) 0x48, (byte) 0x63, (byte) 0xec, (byte) 0xa1, (byte) 0xb6, (byte) 0x42, (byte) 0xa0, (byte) 0x9d,
            (byte) 0x48, (byte) 0x25, (byte) 0xf8, (byte) 0xb3,
            (byte) 0x9d, (byte) 0xd0, (byte) 0xe8, (byte) 0x6a, (byte) 0xf9, (byte) 0x48, (byte) 0x4d, (byte) 0xa1,
            (byte) 0xc2, (byte) 0xba, (byte) 0x86, (byte) 0x30,
            (byte) 0x42, (byte) 0xea, (byte) 0x9d, (byte) 0xb3, (byte) 0x08, (byte) 0x6c, (byte) 0x19, (byte) 0x0e,
            (byte) 0x48, (byte) 0xb3, (byte) 0x9d, (byte) 0x66,
            (byte) 0xeb, (byte) 0x00, (byte) 0x06, (byte) 0xa2, (byte) 0x5a, (byte) 0xee, (byte) 0xa1, (byte) 0x1b,
            (byte) 0x13, (byte) 0x87, (byte) 0x3c, (byte) 0xd7,
            (byte) 0x19, (byte) 0xe6, (byte) 0x55, (byte) 0xbd
        };

        private volatile Shannon _sendCipher = new Shannon();
        private volatile Shannon _recvCipher = new Shannon();

        private volatile int recvNonce;
        private volatile int sendNonce;
        public SpotifyStream(Socket socket) : base(socket)
        {
            ResetNonce();
        }

        public SpotifyStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket)
        {
            ResetNonce();
        }

        public SpotifyStream(Socket socket, FileAccess access) : base(socket, access)
        {
            ResetNonce();
        }

        public SpotifyStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket)
        {
            ResetNonce();
        }

        public bool ConnectToSpotify(ClientHello clientHello, DiffieHellman keys)
        {
            var clientHelloBytes = clientHello.ToByteArray();

            var (accumulator, aPresponseBytes) = WriteAccumulator(clientHelloBytes);

            using var data = ReadApResponseMessage(accumulator, aPresponseBytes, keys);
            if (this.DataAvailable)
            {
                try
                {
                    var scrap = new byte[4];
                    ReadTimeout = 300;
                    var read = Read(scrap, 0, scrap.Length);
                    if (read == scrap.Length)
                    {
                        var length = (scrap[0] << 24) | (scrap[1] << 16) | (scrap[2] << 8) | (scrap[3] & 0xFF);
                        var payload = new byte[length - 4];
                        this.ReadComplete(payload, 0, payload.Length);
                        var failed = APResponseMessage.Parser.ParseFrom(payload)?.LoginFailed;
                        throw new SpotifyAuthenticatedException(failed);
                    }
                    else if (read > 0)
                    {
                        throw new Exception("Read unknown data!");
                    }
                }
                catch (Exception x)
                {
                    // ignored
                }
            }
            ReadTimeout = Timeout.Infinite;

            using (_authLock.Lock())
            {
                _sendCipher = new Shannon();
                _sendCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x14, 0x34));

                _recvCipher = new Shannon();
                _recvCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x34, 0x54));
                _authLockEventWaitHandle.Set();
            }
            accumulator.Dispose();
            return true;
        }

        public void Authenticate(
            LoginCredentials credentials,
            SpotifyConfiguration config)
        {
            if (_recvCipher == null) throw new Exception("Not connected");
            if (_sendCipher == null) throw new Exception("Not Connected");

            var clientResponseEncrypted = GetNewEncrypted(credentials, config);

            SendUnchecked(MercuryPacketType.Login,
                clientResponseEncrypted.ToByteArray(), CancellationToken.None);

            var packet = Receive(CancellationToken.None);
            switch (packet.Cmd)
            {
                case MercuryPacketType.APWelcome:
                {
                    var apWelcome = APWelcome.Parser.ParseFrom(packet.Payload);
                    ListenersHolder
                        .SpotifySessionConcurrentDictionary
                        .ForEach(z => z.ApWelcomeReceived(apWelcome));

                    var bytes0X0F = new byte[20];
                    (new Random()).NextBytes(bytes0X0F);
                    SendUnchecked(MercuryPacketType.Unknown_0x0f,
                        bytes0X0F,
                        CancellationToken.None);

                    using var preferredLocale = new MemoryStream(18 + 5);
                    preferredLocale.WriteByte((byte) 0x0);
                    preferredLocale.WriteByte((byte) 0x0);
                    preferredLocale.WriteByte((byte) 0x10);
                    preferredLocale.WriteByte((byte) 0x0);
                    preferredLocale.WriteByte((byte) 0x02);
                    preferredLocale.Write("preferred-locale");
                    preferredLocale.Write(config.PreferredLocale);

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

                    break;
                }
                case MercuryPacketType.AuthFailure:
                    var apFailed = APLoginFailed.Parser.ParseFrom(packet.Payload);
                    ListenersHolder
                        .SpotifySessionConcurrentDictionary
                        .ForEach(z => z.ApLoginFailedReceived(apFailed));
                    throw new SpotifyAuthenticatedException(apFailed);
                default:
                    throw new MercuryUnknownCmdException(packet);
            }
        }

        public MercuryPacket Receive(CancellationToken cts)
        {
            using (_recvLock.Lock(cts))
            {
                _recvCipher.nonce(recvNonce.ToByteArray());
                Interlocked.Increment(ref recvNonce);

                var headerBytes = new byte[3];
                this.ReadComplete(headerBytes, 0, headerBytes.Length);
                _recvCipher.decrypt(headerBytes);

                var cmd = headerBytes[0];
                var payloadLength = (short)((headerBytes[1] << 8) | (headerBytes[2] & 0xFF));

                var payloadBytes = new byte[payloadLength];
                this.ReadComplete(payloadBytes, 0, payloadBytes.Length);
                _recvCipher.decrypt(payloadBytes);

                var mac = new byte[4];
                this.ReadComplete(mac, 0, mac.Length);

                var expectedMac = new byte[4];
                _recvCipher.finish(expectedMac);
                return new MercuryPacket((MercuryPacketType)cmd, payloadBytes);
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
                var payloadLengthAsByte = BitConverter.GetBytes((short)payload.Length).Reverse().ToArray();
                using var yetAnotherBuffer = new MemoryStream(3 + payload.Length);
                yetAnotherBuffer.WriteByte((byte)cmd);
                yetAnotherBuffer.Write(payloadLengthAsByte, 0, payloadLengthAsByte.Length);
                yetAnotherBuffer.Write(payload, 0, payload.Length);

                _sendCipher.nonce(sendNonce.ToByteArray());
                Interlocked.Increment(ref sendNonce);

                var bufferBytes = yetAnotherBuffer.ToArray();
                _sendCipher.encrypt(bufferBytes);

                var fourBytesBuffer = new byte[4];
                _sendCipher.finish(fourBytesBuffer);
                Write(bufferBytes, 0, bufferBytes.Length);
                Write(fourBytesBuffer, 0, fourBytesBuffer.Length);
                Flush();
            }
        }

        private (MemoryStream Accumulator, byte[] APresponseBytes) WriteAccumulator(byte[] clientHelloBytes)
        {

            WriteByte(0x00);
            WriteByte(0x04);
            WriteByte(0x00);
            WriteByte(0x00);
            WriteByte(0x00);
            Flush();
            var length = 2 + 4 + clientHelloBytes.Length;
            var bytes = BitConverter.GetBytes(length);

            WriteByte(bytes[0]);
            Write(clientHelloBytes, 0, clientHelloBytes.Length);
            Flush();

            var buffer = new byte[1000];
            var len = int.Parse(Read(buffer, 0, buffer.Length).ToString());
            var tmp = new byte[len];
            Array.Copy(buffer, tmp, len);

            tmp = tmp.Skip(4).ToArray();
            var accumulator = new MemoryStream();
            accumulator.WriteByte(0x00);
            accumulator.WriteByte(0x04);

            var lnarr = length.ToByteArray();
            accumulator.Write(lnarr, 0, lnarr.Length);
            accumulator.Write((byte[])clientHelloBytes, 0, clientHelloBytes.Length);

            var lenArr = len.ToByteArray();
            accumulator.Write(lenArr, 0, lenArr.Length);
            accumulator.Write((byte[])tmp, 0, tmp.Length);
            return (accumulator, tmp);
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
                var temp = new[] { (byte)i };
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

            WriteByte(0x00);
            WriteByte(0x00);
            WriteByte(0x00);
            var bytesb = BitConverter.GetBytes(len);
            WriteByte(bytesb[0]);
            Write(clientResponsePlaintextBytes, 0, clientResponsePlaintextBytes.Length);
            Flush();
            return data;
        }

        private ClientResponseEncrypted GetNewEncrypted(
            LoginCredentials credentials,
            SpotifyConfiguration config)
        {
            return new ClientResponseEncrypted
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
    }
}
