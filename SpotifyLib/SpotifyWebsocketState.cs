﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using SpotifyLib.Helpers;
using SpotifyLib.Models;
using SpotifyLib.Models.Response;
using Websocket.Client;

namespace SpotifyLib
{
    public interface IAudioOutput
    {
    }

    public class SpotifyWebsocketState : IDisposable
    {
        private readonly WebsocketClient _client;
        private readonly SpotifyConnectionState _conState;
        private readonly CancellationToken _ct;
        private readonly IAudioOutput? _audioOutput;
        private readonly CancellationTokenSource _pingTokenSource;
        private readonly CancellationTokenSource _linkedToken;
        private readonly IDisposable[] _disposables = new IDisposable[2];
        private Cluster _latestCluster;
        private ISpotifyDevice _activeDevce;

        internal SpotifyWebsocketState(
            WebsocketClient client,
            SpotifyConnectionState conState,
            CancellationToken ct, IAudioOutput? audioOutput)
        {
            _pingTokenSource = new CancellationTokenSource();
            _ct = ct;
            _audioOutput = audioOutput;
            _client = client;
            _conState = conState;
            _disposables[0] = _client.MessageReceived
                .Where(msg => msg.Text != null)
                .Where(msg => msg.Text.StartsWith("{"))
                .Select(a => (GetHeaders(JObject.Parse(a.Text)), a.Text))
                .Subscribe(OnMessageReceived);
            _disposables[1] = _client.DisconnectionHappened.Subscribe(async info =>
            {
                Debug.WriteLine(info.Exception.ToString());
                WaitForConnectionId.Reset();
                _client.Url = await GetUri(_conState, CancellationToken.None);
                await _client.Start();
            });

            _linkedToken =
                CancellationTokenSource.CreateLinkedTokenSource(ct, _pingTokenSource.Token);
            _ = Task.Run(async () =>
            {
                while (!_linkedToken.Token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), _linkedToken.Token);
                    try
                    {
                        _client.Send("{\"type\":\"ping\"}");
                    }
                    catch (Exception x)
                    {
                        Debugger.Break();
                    }
                }

            }, _linkedToken.Token);


            WaitForConnectionId = new ManualResetEvent(false);
            PutState = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Device
                {
                    DeviceInfo = new DeviceInfo()
                    {
                        CanPlay = true,
                        Volume = 65536,
                        Name = _conState.Config.DeviceName,
                        DeviceId = _conState.Config.DeviceId,
                        DeviceType = DeviceType.Computer,
                        DeviceSoftwareVersion = "Spotify-11.1.0",
                        SpircVersion = "3.2.6",
                        Capabilities = new Capabilities
                        {
                            CanBePlayer = true,
                            GaiaEqConnectId = true,
                            SupportsLogout = true,
                            VolumeSteps = 64,
                            IsObservable = true,
                            CommandAcks = true,
                            SupportsRename = false,
                            SupportsPlaylistV2 = true,
                            IsControllable = true,
                            SupportsCommandRequest = true,
                            SupportsTransferCommand = true,
                            SupportsGzipPushes = true,
                            NeedsFullPlayerState = false,
                            SupportedTypes =
                            {
                                "audio/episode",
                                "audio/track"
                            }
                        }
                    },
                    PlayerState = new PlayerState
                    {
                        PlaybackSpeed = 1.0,
                        SessionId = string.Empty,
                        PlaybackId = string.Empty,
                        Suppressions = new Suppressions(),
                        ContextRestrictions = new Restrictions(),
                        Options = new ContextPlayerOptions
                        {
                            RepeatingTrack = false,
                            ShufflingContext = false,
                            RepeatingContext = false
                        },
                        Position = 0,
                        PositionAsOfTimestamp = 0,
                        IsPlaying = false,
                        IsSystemInitiated = true
                    }
                }
            };
        }

        internal PutStateRequest PutState { get; }
        internal HttpClient PutHttpClient { get; private set; }
        internal ManualResetEvent WaitForConnectionId { get; }
        public string ConnectionId { get; private set; }
        public event EventHandler<Cluster> ClusterUpdated;
        public event EventHandler<(ISpotifyDevice? Old, ISpotifyDevice? New)> ActiveDeviceChanged;

        public Cluster LatestCluster
        {
            get => _latestCluster;
            private set
            {
                _latestCluster = value;
                ActiveDevice = new RemoteSpotifyDevice(value.Device[value.ActiveDeviceId], _conState.Config);
                ClusterUpdated?.Invoke(this, value);
            }
        }

        public ISpotifyDevice ActiveDevice
        {
            get => _activeDevce;
            private set
            {
                if (value?.DeviceId != _activeDevce?.DeviceId)
                {
                    ActiveDeviceChanged?.Invoke(this, (_activeDevce, value));
                }
                _activeDevce = value;
            }
        }

        private async void OnMessageReceived((Dictionary<string, string>, string Text) obj)
        {
            if (obj.Item1 == null) return;
            if (obj.Item1.ContainsKey("Spotify-Connection-Id"))
            {
                ConnectionId =
                    HttpUtility.UrlDecode(obj.Item1["Spotify-Connection-Id"],
                        Encoding.UTF8);
                Debug.WriteLine($"new con id: {ConnectionId}");
                PutHttpClient?.Dispose();
                PutHttpClient = new HttpClient(new LoggingHandler(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                }, _conState))
                {
                    BaseAddress = new Uri(await ApResolver.GetClosestSpClient())
                };
                PutHttpClient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", ConnectionId);
                //send device hello.
                var initial = await 
                    UpdateState(PutStateReason.NewDevice);
                LatestCluster = Cluster.Parser.ParseFrom(initial);
                WaitForConnectionId.Set();
            }
            else
            {
                Debug.WriteLine($"Incoming ws message..");
                var wsMessage = AdaptToWsMessage(JObject.Parse(obj.Text));
                switch (wsMessage)
                {
                    case SpotifyWebsocketMessage msg:
                        if (msg.Uri.StartsWith("hm://connect-state/v1/cluster"))
                        {
                            var update = ClusterUpdate.Parser.ParseFrom(msg.Payload);
                            LatestCluster = update.Cluster;
                            ClusterUpdated?.Invoke(this, update.Cluster);
                        }
                        break;
                    case SpotifyWebsocketRequest req:
                        break;
                }
            }
        }

        internal async Task<byte[]> UpdateState(
            PutStateReason reason, int playertime = -1)
        {
            if (playertime == -1) PutState.HasBeenPlayingForMs = 0L;
            else PutState.HasBeenPlayingForMs = (ulong)playertime;

            PutState.PutStateReason = reason;
            PutState.ClientSideTimestamp = (ulong)TimeHelper.CurrentTimeMillisSystem;

            var asBytes = PutState.ToByteArray();
            using var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                gzip.Write(asBytes, 0, asBytes.Length);
            }

            ms.Position = 0;
            var content = new StreamContent(ms);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
            content.Headers.ContentEncoding.Add("gzip");

            var res = await PutHttpClient
                .PutAsync($"/connect-state/v1/devices/{_conState.Config.DeviceId}", content, _ct);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsByteArrayAsync();
            }

            throw new HttpRequestException(res.StatusCode.ToString());
        }


        /// <summary>
        /// Attemps to connect to the wss:// socket to listen for remote commands/requests.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="device"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static Task<SpotifyWebsocketState> ConnectToRemote(
            SpotifyConnectionState connection,
            IAudioOutput? device = null,
            CancellationToken ct = default)
        {
            return Task.Run(async () =>
            {
                var client = new WebsocketClient(
                    await GetUri(connection, ct))
                {
                    IsReconnectionEnabled = false
                };
                var newInstance = new SpotifyWebsocketState(client, connection, ct, device);
                await newInstance._client.StartOrFail();
                newInstance.WaitForConnectionId.WaitOne();
                return newInstance;
            }, ct);
        }
        private static async
            Task<Uri> GetUri(SpotifyConnectionState st,
                CancellationToken ct = default)
        {
            var token = await st.GetToken(ct);
            var socketUrl =
                $"wss://{(await ApResolver.GetClosestDealerAsync()).Replace("https://", string.Empty)}/" +
                $"?access_token={token.AccessToken}";
            return new Uri(socketUrl);
        }
        private static Dictionary<string, string> GetHeaders(JObject msg)
        {
            var headers = msg["headers"] as JObject;
            return headers?.ToObject<Dictionary<string, string>>();
        }
        private static ISpotifyWsMsg
        AdaptToWsMessage(JObject obj)
        {
            switch (obj["type"]?.ToString())
            {
                case "ping":
                    return new Ping();
                    break;
                case "pong":
                    return new Pong();
                    break;
                case "request":
                {
                    Debug.Assert(obj != null, nameof(obj) + " != null");
                    var mid = obj["message_ident"]?.ToString();
                    var key = obj["key"]?.ToString();
                    var headers = GetHeaders(obj);
                    var payload = obj["payload"];

                    using var @in = new MemoryStream();
                    using var outputStream =
                        new MemoryStream(Convert.FromBase64String(payload["compressed"].ToString()));
                    if (headers["Transfer-Encoding"]?.Equals("gzip") ?? false)
                    {
                        using var decompressionStream = new GZipStream(outputStream, CompressionMode.Decompress);
                        decompressionStream.CopyTo(@in);
                        Debug.WriteLine($"Decompressed");
                        var jsonStr = Encoding.Default.GetString(@in.ToArray());
                        payload = JObject.Parse(jsonStr);

                    }

                    var pid = payload["message_id"].ToObject<int>();
                    var sender = payload["sent_by_device_id"]?.ToString();

                    var command = payload["command"];
                    Debug.WriteLine("Received request. mid: {0}, key: {1}, pid: {2}, sender: {3}", mid, key, pid,
                        sender);

                    return new SpotifyWebsocketRequest(mid, pid, sender, (JObject) command);
                }
                    break;
                case "message":
                {
                        var headers = GetHeaders(obj);
                        var uri = obj["uri"]?.ToString();
                        var payloads = (JArray)obj["payloads"];
                        byte[] decodedPayload = null;
                        if (payloads != null)
                        {
                            if (headers.ContainsKey("Content-Type")
                                && (headers["Content-Type"].Equals("application/json") ||
                                    headers["Content-Type"].Equals("text/plain")))
                            {
                                if (payloads.Count > 1) throw new InvalidOperationException();
                                decodedPayload = Encoding.Default.GetBytes(payloads[0].ToString());
                            }
                            else if (headers.Any())
                            {
                                var payloadsStr = new string[payloads.Count];
                                for (var i = 0; i < payloads.Count; i++) payloadsStr[i] = payloads[i].ToString();
                                var x = string.Join("", payloadsStr);
                                using var @in = new MemoryStream();
                                using var outputStream = new MemoryStream(Convert.FromBase64String(x));
                                if (headers.ContainsKey("Transfer-Encoding")
                                    && (headers["Transfer-Encoding"]?.Equals("gzip") ?? false))
                                {
                                    using var decompressionStream = new GZipStream(outputStream, CompressionMode.Decompress);
                                    decompressionStream.CopyTo(@in);
                                    Debug.WriteLine("Decompressed");
                                }

                                decodedPayload = @in.ToArray();
                            }
                            else
                            {
                                Debug.WriteLine($"Unknown message; Possibly playlist update.. {uri}");
                            }
                        }
                        else
                        {
                            decodedPayload = new byte[0];
                        }

                        return new SpotifyWebsocketMessage(uri, headers, decodedPayload);
                    }
                default:
                    Debugger.Break();
                    throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _conState?.Dispose();
            _pingTokenSource?.Dispose();
            _linkedToken?.Dispose();
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
            PutHttpClient?.Dispose();
            WaitForConnectionId?.Dispose();
        }
    }
}