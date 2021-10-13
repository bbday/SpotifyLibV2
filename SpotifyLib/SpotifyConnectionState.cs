using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using Nito.AsyncEx;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Spotify;
using Spotify.Extendedmetadata.Proto;
using Spotify.Playlist4.Proto;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Models;
using SpotifyLib.Models.Response;
using SpotifyLib.Models.Response.SimpleItems;

namespace SpotifyLib
{
    public interface IAuthenticator
    {
        LoginCredentials GetCredentials();
    }

    public readonly struct StoredAuthenticator : IAuthenticator
    {
        private readonly string _username;
        private readonly ByteString _data;
        private readonly AuthenticationType _type;

        public StoredAuthenticator(string username,
            ByteString data,
            AuthenticationType type)
        {
            _username = username;
            _data = data;
            _type = type;
        }

        public LoginCredentials GetCredentials()
            => new LoginCredentials
            {
                Username = _username,
                AuthData = _data,
                Typ = _type
            };
    }

    public readonly struct UserpassAuthenticator : IAuthenticator
    {
        private readonly string _username;
        private readonly string _password;

        public UserpassAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public LoginCredentials GetCredentials()
            => new LoginCredentials
            {
                Username = _username,
                AuthData = ByteString.CopyFromUtf8(_password),
                Typ = AuthenticationType.AuthenticationUserPass
            };
    }

    public static class SpotifyClientMethods
    {
        /// <summary>
        /// Opens a TCP Connection to one of the AP(Access Point)s and attempts to authenticate the user.
        /// </summary>
        /// <param name="authenticator">Type of authenticators to use. Can be for exampel <see cref="UserpassAuthenticator"/> or <see cref="StoredAuthenticator"/></param>
        /// <param name="config">Holds data like the config, device name and device id. See <see cref="SpotifyConfig"/>, for a default config use <see cref="SpotifyConfig.Default()"/></param>
        /// <param name="ct">A <see cref="CancellationToken"/> for the asynchronous task(s).</param>
        /// <returns>An isntance of type <see cref="SpotifyConnectionState"/></returns>
        /// <exception cref="SpotifyConnectionException">Thrown whenever an issue with the CONNECTION between Spotify and your machine (not authorization) occurs.</exception>
        /// <exception cref="SpotifyAuthenticationException">Thrown whenever a user enters bad credentials. Check <see cref="SpotifyAuthenticationException.Failed"/> for more details.
        /// Specifically <see cref="APLoginFailed.ErrorCode"/></exception>
        /// <exception cref="UnknownDataException">Thrown whenever junk is read. This usually indicates something is wrong with the library, as this should not be thrown!</exception>
        /// <exception cref="IOException">Thrown whenever an issue occurs with the underlying socket. (can be independent of Spotify)</exception>
        public static async Task<SpotifyConnectionState>
            Authenticate(IAuthenticator authenticator,
                SpotifyConfig config,
                CancellationToken ct = default)
        {
            var a = await ApResolver.GetClosestAccessPoint(ct);
            var (host, port) = a.First();
            Debug.WriteLine($"Fetched: {host}:{port} as AP.");
            return await
                SpotifyConnectionState.Connect(host, port, authenticator, config, ct);
        }
    }

    public interface ISpotifyConnectionState
    {
        Task<T> SendAndReceiveAsJson<T>(
            string mercuryUri,
            MercuryRequestType type = MercuryRequestType.Get,
            CancellationToken ct = default);

        Task<string> SendAndReceiveAsJsonString(
            string mercuryUri,
            MercuryRequestType type = MercuryRequestType.Get,
            CancellationToken ct = default);

        Task<MercuryToken> GetToken(CancellationToken ct);

        APWelcome ApWelcome { get; }
        Task<IEnumerable<View<ISpotifyItem>>> GetHome(CancellationToken ct = default);
        Task<View<ISpotifyItem>> GetView(string view, CancellationToken ct = default);
        Task<FullPlaylistEverything> FetchEverythingAboutAPlaylist(SpotifyId id, CancellationToken ct = default);
        Task<BatchedExtensionResponse> FetchTracks(IEnumerable<SpotifyId> @select, CancellationToken ct = default);
    }
    public class SpotifyConnectionState : ISpotifyConnectionState, IDisposable
    {
        public readonly string Host;
        public readonly int Port;
        public readonly IAuthenticator Authenticator;
        public readonly SpotifyConfig Config;

        internal event EventHandler DisposedCalled;

        internal SpotifyConnectionState(string host, int port, IAuthenticator authenticator, SpotifyConfig config)
        {
            Host = host;
            Port = port;
            Authenticator = authenticator;
            Config = config;
        }

        internal volatile int AudioKeySequence;
        internal AsyncLock AudioKeyLock { get; set; }
        internal AsyncLock TokenLock { get; set; }
        internal AsyncLock SendLock { get; set; }
        internal AsyncLock ReceiveLock { get; set; }
        internal volatile int Sequence;
        internal TcpClient TcpConnection { get; private set; }
        internal Shannon RecvCipher { get; private set; }
        internal Shannon SendCipher { get; private set; }
        public APWelcome ApWelcome { get; private set; }

        public async Task<IEnumerable<View<ISpotifyItem>>> GetHome(CancellationToken ct = default)
        {
            //TODO: Country
            var aTask = "https://api.spotify.com"
                .AppendPathSegments("v1", "views", "personalized-recommendations")
                .SetQueryParam("timestamp", "2021-07-18T22:43:42.597Z")
                .SetQueryParam("platform", "web")
                .SetQueryParam("content_limit", "10")
                .SetQueryParam("limit", "20")
                .SetQueryParam("types", "album,playlist,artist,show,station,episode")
                .SetQueryParam("image_style", "gradient_overlay")
                .SetQueryParam("country", "JP")
                .SetQueryParam("locale", $"{Config.Locale}")
                .SetQueryParam("market", "from_token")
                .WithOAuthBearerToken((await GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);

            var a =
                ViewDeserializerHelpers.Deserialize(await aTask);
            return a.Content.Items?.Where(z => z.Id != "nft-home-recently-played").ToList() ??
                   new List<View<ISpotifyItem>>();
        }

        public async Task<View<ISpotifyItem>> GetView(string view, CancellationToken ct = default)
        {
            var bTask = "https://api.spotify.com"
                .AppendPathSegments("v1", "views", view)
                .SetQueryParam("limit", "50")
                .SetQueryParam("types", "album,playlist,artist,show,station,episode")
                .SetQueryParam("locale", $"{Config.Locale}")
                .SetQueryParam("market", "from_token")
                .WithOAuthBearerToken((await GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);
            return ViewDeserializerHelpers.DeserializeSingleView(await bTask);
        }

        public async Task<FullPlaylistEverything> FetchEverythingAboutAPlaylist(SpotifyId id,
            CancellationToken ct = default)
        {
            static T Deserialize_Int<T>(byte[] data)
            {
                return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(data),
                    ViewDeserializerHelpers.opts);
            }

            const string fields =
                "followers(total), images, owner(display_name, id, images, uri), public, uri";
            const string uri = "https://api.spotify.com/v1/playlists";

            var fetchSimplePlaylistTask = uri
                .AppendPathSegment(id.Id)
                .SetQueryParam("fields", fields)
                .WithOAuthBearerToken((await GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);

            var spClient = await ApResolver.GetClosestSpClient();
            var fetchMetadataTask = spClient.AppendPathSegments("playlist", "v2", "playlist", id.Id)
                .WithOAuthBearerToken((await GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);
            await Task.WhenAll(fetchMetadataTask, fetchSimplePlaylistTask);

            var selectedListContent = SelectedListContent.Parser.ParseFrom(await fetchMetadataTask);
            return new FullPlaylistEverything(selectedListContent,
                Deserialize_Int<SimplePlaylist>(await fetchSimplePlaylistTask));
        }

        public async Task<BatchedExtensionResponse> FetchTracks(IEnumerable<SpotifyId> @select,
            CancellationToken ct = default)
        {
            var request = new Spotify.Extendedmetadata.Proto.BatchedEntityRequest();
            request.EntityRequest.AddRange(@select.Select(a => new EntityRequest
            {
                EntityUri = a.Uri,
                Query =
                {
                    new ExtensionQuery
                    {
                        ExtensionKind = a.Type switch
                        {
                            AudioItemType.Track => ExtensionKind.TrackV4,
                            AudioItemType.Episode => ExtensionKind.EpisodeV4,
                            _ => ExtensionKind.UnknownExtension
                        }
                    }
                }
            }));
            request.Header = new BatchedEntityRequestHeader
            {
                Catalogue = "premium",
                Country = "JP"
            };
            var spclient = await ApResolver.GetClosestSpClient();
            var metadataResponse = await spclient
                .AppendPathSegments("extended-metadata", "v0", "extended-metadata")
                .WithOAuthBearerToken((await GetToken(ct)).AccessToken)
                .PostAsync(new ByteArrayContent(request.ToByteArray()), cancellationToken: ct);
            var bts = await metadataResponse.GetBytesAsync();
            return BatchedExtensionResponse.Parser.ParseFrom(bts);
        }

        public static Task<SpotifyConnectionState> Connect(string host, int port, IAuthenticator authenticator,
            SpotifyConfig config,
            CancellationToken ct = default) => Task.Run(async () =>
        {
            var connection = new SpotifyConnectionState(host, port, authenticator, config);
            connection.PackageListenerToken?.Cancel();
            connection.PackageListenerToken?.Dispose();
            connection.TcpConnection?.Dispose();
            connection._waiters =
                new ConcurrentDictionary<long, (AsyncAutoResetEvent Waiter, MercuryResponse? Response)>();
            connection._audioKeys = new ConcurrentDictionary<int, (AsyncAutoResetEvent Waiter, byte[])>();
            connection._partials = new ConcurrentDictionary<long, List<byte[]>>();
            connection.Tokens = new ConcurrentBag<MercuryToken>();
            connection.TcpConnection = new TcpClient(connection.Host, connection.Port)
            {
                ReceiveTimeout = 500
            };

            #region Client Hello

            connection.TokenLock = new AsyncLock();
            connection.SendLock = new AsyncLock();
            connection.ReceiveLock = new AsyncLock();
            connection.AudioKeyLock = new AsyncLock();
            connection.AudioKeySequence = 0;
            connection.Sequence = 0;

            var keys = new DiffieHellman();
            var ke = keys.PublicKeyArray();
            var clientHello = ClientHello(ke);

            var clientHelloBytes = clientHello.ToByteArray();
            var networkStream = connection.TcpConnection.GetStream();

            //Write the initial client hello..
            networkStream.WriteByte(0x00);
            networkStream.WriteByte(0x04);
            networkStream.WriteByte(0x00);
            networkStream.WriteByte(0x00);
            networkStream.WriteByte(0x00);
            await networkStream.FlushAsync(ct);

            var length = 2 + 4 + clientHelloBytes.Length;
            var bytes = BitConverter.GetBytes(length);

            networkStream.WriteByte(bytes[0]);
            networkStream.Write(clientHelloBytes, 0, clientHelloBytes.Length);
            await networkStream.FlushAsync(ct);

            var buffer = new byte[1000];

            var len = int.Parse(networkStream.Read(buffer,
                0, buffer.Length).ToString());
            var tmp = new byte[len];
            System.Array.Copy(buffer, tmp, len);

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

            using var data = ReadApResponseMessage(accumulator, tmp,
                keys, networkStream);

            if (networkStream.DataAvailable)
            {
                //if data is available, it could be scrap or a failed login.
                try
                {
                    var scrap = new byte[4];
                    networkStream.ReadTimeout = 300;
                    var read = networkStream.Read(scrap, 0, scrap.Length);
                    if (read == scrap.Length)
                    {
                        var lengthOfScrap = (scrap[0] << 24) | (scrap[1] << 16) | (scrap[2] << 8) |
                                            (scrap[3] & 0xFF);
                        var payload = new byte[length - 4];
                        await networkStream.ReadCompleteAsync(payload, 0, payload.Length, ct);
                        var failed = APResponseMessage.Parser.ParseFrom(payload);
                        throw new SpotifyConnectionException(failed);
                    }

                    if (read > 0) throw new UnknownDataException(scrap);
                }
                catch (Exception x)
                {
                    // ignored
                }
            }

            //Reset network timeout to infinite. This will allow use to wait for messages.
            networkStream.ReadTimeout = Timeout.Infinite;

            var sendCipher = new Shannon();
            sendCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x14, 0x34));

            var recvCipher = new Shannon();
            recvCipher.key(Arrays.CopyOfRange(data.ToArray(), 0x34, 0x54));
            accumulator.Dispose();

            connection.RecvCipher = recvCipher;
            connection.SendCipher = sendCipher;

            #endregion

            #region Authentication

            var credentials = connection.Authenticator.GetCredentials();
            var clientResponseEncrypted =
                GetNewEncrypted(credentials, connection.Config);

            await connection.SendPackageAsync(new MercuryPacket(MercuryPacketType.Login,
                clientResponseEncrypted.ToByteArray()), ct);

            var packet = await
                connection.ReceivePackageAsync(ct);
            switch (packet.Cmd)
            {
                case MercuryPacketType.APWelcome:
                    connection.PackageListenerToken = new CancellationTokenSource();
                    //Start a background function that waits and listens for packages
                    _ = Task.Run(() => connection.WaitForPackages(ct), ct);
                    var apWelcome = APWelcome.Parser.ParseFrom(packet.Payload);
                    await connection.UpdateLocaleAsync(connection.Config.Locale, ct);
                    connection.ApWelcome = apWelcome;
                    return connection;
                    //  return new SpotifyClient(config, apWelcome, tcpClient, authenticator);
                    break;
                case MercuryPacketType.AuthFailure:
                    throw new SpotifyAuthenticationException(APLoginFailed.Parser.ParseFrom(packet.Payload));
                default:
                    throw new UnknownDataException($"Invalid package type: {packet.Cmd}", packet.Payload);
            }

            #endregion
        }, ct);

        public void Dispose()
        {
            TcpConnection?.Dispose();
            PackageListenerToken?.Dispose();
            DisposedCalled?.Invoke(this, EventArgs.Empty);
        }

        private async Task WaitForPackages(CancellationToken cancellationToken)
        {
            using var linked =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, PackageListenerToken.Token);
            while (!linked.IsCancellationRequested)
            {
                try
                {
                    var newPacket = await ReceivePackageAsync(linked.Token);
                    if (!Enum.TryParse(newPacket.Cmd.ToString(), out MercuryPacketType cmd))
                    {
                        Debug.WriteLine(
                            $"Skipping unknown command cmd: {newPacket.Cmd}," +
                            $" payload: {newPacket.Payload.BytesToHex()}");
                        continue;
                    }

                    switch (cmd)
                    {
                        case MercuryPacketType.Ping:
                            Debug.WriteLine("Receiving ping..");
                            try
                            {
                                await SendPackageAsync(new MercuryPacket(MercuryPacketType.Pong,
                                    newPacket.Payload), linked.Token);
                            }
                            catch (IOException ex)
                            {
                                Debug.WriteLine("Failed sending Pong!", ex);
                                Debugger.Break();
                                //TODO: Reconnect
                            }

                            break;
                        case MercuryPacketType.PongAck:
                            break;
                        case MercuryPacketType.MercuryReq:
                        case MercuryPacketType.MercurySub:
                        case MercuryPacketType.MercuryUnsub:
                        case MercuryPacketType.MercuryEvent:
                            //Handle mercury packet..
                            // con.HandleMercury(newPacket);
                            _ = HandleMercuryAsync(newPacket, linked.Token);
                            break;
                        case MercuryPacketType.AesKeyError:
                        case MercuryPacketType.AesKey:
                            _ = HandleAesKey(newPacket, linked.Token);
                            break;
                    }
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x.ToString());
                    ConnectionException?.Invoke(this, x);
                    linked?.Dispose();
                    //TODO: What to do here?
                    break;
                }
            }
        }

        private async Task HandleAesKey(MercuryPacket packet, CancellationToken ct = default)
        {
            using var payload = new MemoryStream(packet.Payload);
            var seq = 0;
            var buffer = packet.Payload;
            seq = packet.Payload.getInt((int) payload.Position, true);
            payload.Seek(4, SeekOrigin.Current);
            if (!_audioKeys.ContainsKey(seq))
            {
                Debug.WriteLine("Couldn't find callback for seq: " + seq);
                return;
            }

            var p = _audioKeys[seq];
            switch (packet.Cmd)
            {
                case MercuryPacketType.AesKey:
                    var key = new byte[16];
                    payload.Read(key, 0, key.Length);
                    p.Item2 = key;
                    break;
                case MercuryPacketType.AesKeyError:
                    var code = packet.Payload.getShort((int) payload.Position, true);
                    payload.Seek(2, SeekOrigin.Current);
                    //TODO: Error
                    Debugger.Break();
                    break;
                default:
                    Debug.WriteLine("Couldn't handle packet, cmd: {0}, length: {1}", packet.Cmd, packet.Payload.Length);
                    break;

            }

            _audioKeys[seq] = p;
            _audioKeys[seq].Waiter.Set();
        }

        private async Task HandleMercuryAsync(MercuryPacket packet, CancellationToken ct = default)
        {
            using var stream = new MemoryStream(packet.Payload);
            int seqLength = packet.Payload.getShort((int) stream.Position, true);
            stream.Seek(2, SeekOrigin.Current);
            long seq = 0;
            var buffer = packet.Payload;
            switch (seqLength)
            {
                case 2:
                    seq = packet.Payload.getShort((int) stream.Position, true);
                    stream.Seek(2, SeekOrigin.Current);
                    break;
                case 4:
                    seq = packet.Payload.getInt((int) stream.Position, true);
                    stream.Seek(4, SeekOrigin.Current);
                    break;
                case 8:
                    seq = packet.Payload.getLong((int) stream.Position, true);
                    stream.Seek(8, SeekOrigin.Current);
                    break;
            }

            var flags = packet.Payload[(int) stream.Position];
            stream.Seek(1, SeekOrigin.Current);
            var parts = packet.Payload.getShort((int) stream.Position, true);
            stream.Seek(2, SeekOrigin.Current);

            _partials.TryGetValue(seq, out var partial);
            partial ??= new List<byte[]>();
            if (!partial.Any() || flags == 0)
            {
                partial = new List<byte[]>();
                _partials.TryAdd(seq, partial);
            }

            Debug.WriteLine("Handling packet, cmd: " +
                            $"{packet.Cmd}, seq: {seq}, flags: {flags}, parts: {parts}");

            for (var j = 0; j < parts; j++)
            {
                var size = packet.Payload.getShort((int) stream.Position, true);
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
                _partials[seq] = partial;
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

            var resp = new MercuryResponse(header, partial, seq);
            switch (packet.Cmd)
            {
                case MercuryPacketType.MercuryReq:
                    var a = _waiters[seq];
                    a.Response = resp;
                    _waiters[seq] = a;
                    a.Waiter.Set();
                    break;
                default:
                    //Debugger.Break();
                    break;
            }
        }

        internal ConcurrentDictionary<int, (AsyncAutoResetEvent Waiter, byte[])> _audioKeys;
        internal ConcurrentDictionary<long, (AsyncAutoResetEvent Waiter, MercuryResponse? Response)> _waiters;

        internal ConcurrentDictionary<long, List<byte[]>>
            _partials;

        internal ConcurrentBag<MercuryToken> Tokens;

        public event EventHandler<Exception> ConnectionException;

        private CancellationTokenSource PackageListenerToken { get; set; }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return TcpConnection != null && TcpConnection.Connected
                                                 && TcpConnection.GetStream().ReadTimeout > -2;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        //TODO
        public Dictionary<string, string> UserAttributes { get; } = new Dictionary<string, string>();
        public string Country { get; private set; }

        internal int SearchSequence;

        /// <summary>
        /// Fire and forget package sending. Somewhat async but mostly relies on Task.Run()
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public Task SendPackageAsync(MercuryPacket packet, CancellationToken ct = default)
            => Task.Run(async () =>
            {
                var payload = packet.Payload;
                var cmd = packet.Cmd;
                using (await SendLock.LockAsync(ct))
                {
                    var payloadLengthAsByte = BitConverter.GetBytes((short) payload.Length).Reverse().ToArray();
                    using var yetAnotherBuffer = new MemoryStream(3 + payload.Length);
                    yetAnotherBuffer.WriteByte((byte) cmd);
                    await yetAnotherBuffer.WriteAsync(payloadLengthAsByte, 0, payloadLengthAsByte.Length, ct);
                    await yetAnotherBuffer.WriteAsync(payload, 0, payload.Length, ct);

                    SendCipher.nonce(SendCipher.Nonce.ToByteArray());
                    Interlocked.Increment(ref SendCipher.Nonce);

                    var bufferBytes = yetAnotherBuffer.ToArray();
                    SendCipher.encrypt(bufferBytes);

                    var fourBytesBuffer = new byte[4];
                    SendCipher.finish(fourBytesBuffer);

                    var networkStream = TcpConnection.GetStream();
                    networkStream.Write(bufferBytes, 0, bufferBytes.Length);
                    networkStream.Write(fourBytesBuffer, 0, fourBytesBuffer.Length);
                    await networkStream.FlushAsync(ct);
                }
            }, ct);

        /// <summary>
        /// Waits and receives a package (blocking function)
        /// </summary>
        /// <returns></returns>
        public Task<MercuryPacket> ReceivePackageAsync(CancellationToken ct) => Task.Run(async () =>
        {
            using (ReceiveLock.Lock(ct))
            {
                RecvCipher.nonce(RecvCipher.Nonce.ToByteArray());
                Interlocked.Increment(ref RecvCipher.Nonce);

                var headerBytes = new byte[3];
                var networkStream = TcpConnection.GetStream();

                await networkStream.ReadCompleteAsync(headerBytes, 0,
                    headerBytes.Length, ct);
                RecvCipher.decrypt(headerBytes);

                var cmd = headerBytes[0];
                var payloadLength = (short) ((headerBytes[1] << 8) | (headerBytes[2] & 0xFF));

                var payloadBytes = new byte[payloadLength];
                await networkStream.ReadCompleteAsync(payloadBytes, 0, payloadBytes.Length, ct);
                RecvCipher.decrypt(payloadBytes);

                var mac = new byte[4];
                await networkStream.ReadCompleteAsync(mac, 0, mac.Length, ct: ct);

                var expectedMac = new byte[4];
                RecvCipher.finish(expectedMac);
                return new MercuryPacket((MercuryPacketType) cmd, payloadBytes);
            }
        }, ct);


        public Task UpdateLocaleAsync(string locale, CancellationToken ct)
        {
            using var preferredLocale = new MemoryStream(18 + 5);
            preferredLocale.WriteByte(0x0);
            preferredLocale.WriteByte(0x0);
            preferredLocale.WriteByte(0x10);
            preferredLocale.WriteByte(0x0);
            preferredLocale.WriteByte(0x02);
            preferredLocale.Write("preferred-locale");
            preferredLocale.Write(locale);
            return SendPackageAsync(new MercuryPacket(MercuryPacketType.PreferredLocale,
                preferredLocale.ToArray()), ct);
        }

        public async Task<T> SendAndReceiveAsJson<T>(
            string mercuryUri,
            MercuryRequestType type = MercuryRequestType.Get,
            CancellationToken ct = default)
        {
            var response = await SendAndReceiveAsResponse(mercuryUri, type, ct);
            if (response is {StatusCode: >= 200 and < 300})
            {
                return Deserialize<T>(response.Value);
            }

            throw new MercuryException(response);
        }

        public async Task<string> SendAndReceiveAsJsonString(string mercuryUri,
            MercuryRequestType type = MercuryRequestType.Get,
            CancellationToken ct = default)
        {
            var response = await SendAndReceiveAsResponse(mercuryUri, type, ct);
            if (response is {StatusCode: >= 200 and < 300})
            {
                return Encoding.UTF8.GetString(response.Value.Payload.SelectMany(a => a)
                    .ToArray());
            }

            throw new MercuryException(response);
        }

        public Task<MercuryResponse?> SendAndReceiveAsResponse(
            string mercuryUri,
            MercuryRequestType type = MercuryRequestType.Get,
            CancellationToken ct = default) => Task.Run(async () =>
        {
            var sequence = Interlocked.Increment(ref Sequence);

            var req = type switch
            {
                MercuryRequestType.Get => RawMercuryRequest.Get(mercuryUri),
                MercuryRequestType.Sub => RawMercuryRequest.Sub(mercuryUri),
                MercuryRequestType.Unsub => RawMercuryRequest.Unsub(mercuryUri)
            };

            var requestPayload = req.Payload.ToArray();
            var requestHeader = req.Header;

            using var bytesOut = new MemoryStream();
            var s4B = BitConverter.GetBytes((short) 4).Reverse().ToArray();
            bytesOut.Write(s4B, 0, s4B.Length); // Seq length

            var seqB = BitConverter.GetBytes(sequence).Reverse()
                .ToArray();
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

            var cmd = type switch
            {
                MercuryRequestType.Sub => MercuryPacketType.MercurySub,
                MercuryRequestType.Unsub => MercuryPacketType.MercuryUnsub,
                _ => MercuryPacketType.MercuryReq
            };

            var wait = new AsyncAutoResetEvent(false);
            _waiters[sequence] = (wait, null);
            await SendPackageAsync(new MercuryPacket(cmd, bytesOut.ToArray()), ct);
            await wait.WaitAsync(ct);

            _waiters.TryRemove(sequence, out var a);
            return a.Response;
        }, ct);

        private static ClientHello ClientHello(byte[] publickey)
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
                    Gc = ByteString.CopyFrom(publickey),
                    ServerKeysKnown = 1
                }
            };
            var nonce = new byte[16];
            new Random().NextBytes(nonce);
            clientHello.ClientNonce = ByteString.CopyFrom(nonce);
            clientHello.Padding = ByteString.CopyFrom(30);

            return clientHello;
        }

        private static MemoryStream ReadApResponseMessage(MemoryStream accumulator,
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
                Modulus = new BigInteger(1, Consts.ServerKey).ToByteArrayUnsigned(),
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

        private static ClientResponseEncrypted GetNewEncrypted(
            LoginCredentials credentials,
            SpotifyConfig config) => new()
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

        #region Other

        private static T Deserialize<T>(MercuryResponse resp) =>
            System.Text.Json.JsonSerializer.Deserialize<T>(
                new ReadOnlySpan<byte>(resp.Payload.SelectMany(z => z).ToArray()), opts);

        #endregion

        private static readonly JsonSerializerOptions opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new UriToSpotifyIdConverter()
            }
        };

        public async Task<MercuryToken> GetToken(CancellationToken ct)
        {
            using (await TokenLock.LockAsync(ct))
            {
                var tokenOrDefault =
                    FindNonExpiredToken();
                if (tokenOrDefault.HasValue) return tokenOrDefault.Value;

                var newToken = await SendAndReceiveAsJson<MercuryToken>(
                    "hm://keymaster/token/authenticated?scope=playlist-read" +
                    $"&client_id={KEYMASTER_CLIENT_ID}&device_id=", ct: ct);
                Tokens.Add(newToken);
                return newToken;
            }
        }

        internal MercuryToken? FindNonExpiredToken()
        {
            var a =
                Tokens.FirstOrDefault(token => !token.IsExpired());
            if (string.IsNullOrEmpty(a.AccessToken)) return null;
            return a;
        }

        const string KEYMASTER_CLIENT_ID = "65b708073fc0480ea92a077233ca87bd";

        public async Task<byte[]> GetAudioKey(ByteString gid, ByteString fileId,
            bool retry = true,
            CancellationToken ct = default)
        {
            using var ctsT = new CancellationTokenSource();
            using var linked =
                CancellationTokenSource.CreateLinkedTokenSource(ct, ctsT.Token);

            var r = 
                Interlocked.Increment(ref AudioKeySequence);
            using var @out = new MemoryStream();
            fileId.WriteTo(@out);
            gid.WriteTo(@out);
            var b = r.ToByteArray();
            @out.Write(b, 0, b.Length);
            @out.Write(ZERO_SHORT, 0, ZERO_SHORT.Length);
           
            var callback = new AsyncAutoResetEvent();
            _audioKeys.TryAdd(r, (callback, null));

            await SendPackageAsync(new MercuryPacket(MercuryPacketType.RequestKey, @out.ToArray()),
                ct);
            
            await callback.WaitAsync(ct);
            var key = _audioKeys[r].Item2;
            if (key != null) 
                return key;
            if (retry) 
                return await GetAudioKey(gid, fileId, false);
            throw new AesKeyException(
                $"Failed fetching audio key! gid: " +
                $"{gid.ToByteArray().BytesToHex()}," +
                $" fileId: {fileId.ToByteArray().BytesToHex()}");
        }

        private static readonly byte[] ZERO_SHORT = new byte[] {0, 0};

    }
}