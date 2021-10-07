using System;
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
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Org.BouncyCastle.Asn1.Ocsp;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLib.Helpers;
using SpotifyLib.Models;
using SpotifyLib.Models.Contexts;
using SpotifyLib.Models.Player;
using SpotifyLib.Models.Response;
using Websocket.Client;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using Restrictions = Connectstate.Restrictions;

namespace SpotifyLib
{
    public enum AudioOutputStateChanged
    {
        Buffering,
        Playing,
        Paused,
        Finished,
        ManualSeek
    }
    public interface IAudioOutput : ISpotifyDevice
    {
        Task IncomingStream(ChunkedStream entry);
        void Resume(long position = -1);
        void Pause();
        void Seek(double d);
        event EventHandler<AudioOutputStateChanged> AudioOutputStateChanged;
        int Position { get; }
        /// <summary>
        /// User is responsible for trying to cache the stream.
        /// You are allowed to return null.
        /// This forces the lib to fetch the stream from the spotify api.
        /// </summary>
        /// <param name="playable"></param>
        /// <returns></returns>
        Task<ChunkedStream?> GetCachedStream(SpotifyId playable);
    }

    public class SpotifyWebsocketState : IDisposable
    {
        private readonly WebsocketClient _client;
        internal readonly SpotifyConnectionState ConState;
        private readonly CancellationToken _ct;
        internal readonly IAudioOutput? AudioOutput;
        private readonly CancellationTokenSource _pingTokenSource;
        private readonly CancellationTokenSource _linkedToken;
        private readonly IDisposable[] _disposables = new IDisposable[2];
        private Cluster _latestCluster;
        private ISpotifyDevice _activeDevce;
        private readonly SpotifyConnectState _connectStateHolder;
        internal SpotifyWebsocketState(
            WebsocketClient client,
            SpotifyConnectionState conState,
            CancellationToken ct, IAudioOutput? audioOutput)
        {
            _pingTokenSource = new CancellationTokenSource();
            _ct = ct;
            AudioOutput = audioOutput;
            _client = client;
            ConState = conState;
            _disposables[0] = _client.MessageReceived
                .Where(msg => msg.Text != null)
                .Where(msg => msg.Text.StartsWith("{"))
                .Select(a => (GetHeaders(JObject.Parse(a.Text)), a.Text))
                .Subscribe(OnMessageReceived);
            _disposables[1] = _client.DisconnectionHappened.Subscribe(async info =>
            {
                Debug.WriteLine(info.Exception.ToString());
                WaitForConnectionId.Reset();
                _client.Url = await GetUri(ConState, CancellationToken.None);
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

            _connectStateHolder = new SpotifyConnectState(this);
        }

        internal HttpClient PutHttpClient { get; private set; }
        internal ManualResetEvent WaitForConnectionId { get; }
        public string ConnectionId { get; private set; }
        public event EventHandler<Cluster> ClusterUpdated;
        public event EventHandler<(ISpotifyDevice? Old, ISpotifyDevice? New)> ActiveDeviceChanged;
        public event 
            EventHandler<List<RemoteSpotifyDevice>> DevicesAvailableChanged;

        private List<RemoteSpotifyDevice> _avaSpotifyDevices { get; set; }
        public Cluster LatestCluster
        {
            get => _latestCluster;
            private set
            {
                _latestCluster = value;
                if (string.IsNullOrEmpty(value.ActiveDeviceId))
                {
                    ActiveDevice = AudioOutput;
                }
                else
                {
                    ActiveDevice = new RemoteSpotifyDevice(value.Device[value.ActiveDeviceId], ConState.Config);
                }

                var newDevices = value
                    .Device
                    .Select(a => new RemoteSpotifyDevice(a.Value, ConState.Config))
                    .ToList();
                if (value.Device
                    .All(a =>
                        newDevices.Any(j => j.DeviceId == a.Key)))
                {
                    if(newDevices
                        .All(a =>
                            value.Device.Any(j => j.Key == a.DeviceId)))
                    {
                        //nothing changed..
                    }
                    else
                    {
                        DevicesAvailableChanged?.Invoke(this, newDevices);
                    }
                }
                else
                {
                    //changed.
                    DevicesAvailableChanged?.Invoke(this, newDevices);

                }

                _avaSpotifyDevices = newDevices;
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

        public bool CanSkipNext => !LatestCluster.PlayerState.Restrictions?.DisallowSkippingNextReasons
            .Any() ?? false;
        public bool CanSkipPrevious => !LatestCluster.PlayerState.Restrictions?.DisallowSkippingPrevReasons
            .Any() ?? false;
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
                }, ConState))
                {
                    BaseAddress = new Uri(await ApResolver.GetClosestSpClient())
                };
                PutHttpClient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", ConnectionId);
                //send device hello.
                var initial = await 
                    _connectStateHolder.UpdateState(PutStateReason.NewDevice, _connectStateHolder.State);
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
                        var result =
                            await Task.Run(() => OnRequest(req));
                        SendReply(req.Key, result);
                        Debug.WriteLine("Handled request. key: {0}, result: {1}", req.Key, result);
                        break;
                }
            }
        }

        internal async Task<RequestResult> OnRequest(SpotifyWebsocketRequest req)
        {
            _connectStateHolder.PutState.LastCommandMessageId = (uint) req.Pid;
            _connectStateHolder.PutState.LastCommandSentByDeviceId = req.Sender;
            var endpoint = req.Command["endpoint"].ToString()
                .StringToEndPoint();
            var cmd = new CommandBody(req.Command);
            switch (endpoint)
            {
                case Endpoint.Transfer:
                    try
                    {
                        await HandleTransferState(TransferState.Parser.ParseFrom(cmd.Data));
                        return RequestResult.Success;
                    }
                    catch (Exception x)
                    {
                        //TODO: Notify user?
                        Debug.WriteLine(x.ToString());
                        return RequestResult.UpstreamError;
                    }
                case Endpoint.Resume:
                    _connectStateHolder.SetState(true, false, false);
                    AudioOutput.Resume();
                    _connectStateHolder.UpdateState(PutStateReason.PlayerStateChanged, _connectStateHolder.State,
                        AudioOutput.Position);
                    return RequestResult.Success;
                    break;
                case Endpoint.Pause:
                    _connectStateHolder.SetState(true, true, false);
                    AudioOutput.Pause();
                    _connectStateHolder.UpdateState(PutStateReason.PlayerStateChanged, _connectStateHolder.State,
                        AudioOutput.Position);
                    return RequestResult.Success;
                    break;
                case Endpoint.SeekTo:
                    AudioOutput.Seek(cmd.Obj.Value<int>("value"));
                    _connectStateHolder.UpdateState(PutStateReason.PlayerStateChanged, _connectStateHolder.State,
                        AudioOutput.Position);
                    return RequestResult.Success;
                    break;
                default:
                    return RequestResult.DeviceDoesNotSupportCommand;
            }

        }

        private async Task HandleTransferState(TransferState cmd)
        {
            Debug.WriteLine($"Loading context (transfer): {cmd.CurrentSession.Context.Uri}");

            var ps = cmd.CurrentSession;
            _connectStateHolder.State.PlayOrigin = ProtoUtils.ConvertPlayOrigin(ps.PlayOrigin);
            _connectStateHolder.State.Options = ProtoUtils.ConvertPlayerOptions(cmd.Options);
            var sessionId = _connectStateHolder.SetContext(ps.Context);

            var playback = cmd.Playback;
            try
            {
                await _connectStateHolder.TracksKeeper.InitializeFrom(list => list.FindIndex(a =>
                        (a.HasUid && ps.CurrentUid.Equals(a.Uid)) || ProtoUtils.TrackEquals(a, playback.CurrentTrack)),
                    playback.CurrentTrack, cmd.Queue);
            }
            catch (IllegalStateException ex)
            {
                Debug.WriteLine(ex.ToString());
                await _connectStateHolder.TracksKeeper.InitializeStart();
            }

            _connectStateHolder.State.PositionAsOfTimestamp = playback.PositionAsOfTimestamp;
            if (playback.IsPaused) _connectStateHolder.State.Timestamp = TimeHelper.CurrentTimeMillisSystem;
            else _connectStateHolder.State.Timestamp = playback.Timestamp;


            await _connectStateHolder
                .LoadSession(sessionId, !cmd.Playback.IsPaused, true);
        }

        internal void SetDeviceIsActive(bool active)
        {
            if (active)
            {
                long now = TimeHelper.CurrentTimeMillisSystem;
                _connectStateHolder.PutState.IsActive = true;
                _connectStateHolder.PutState.StartedPlayingAt = (ulong) now;
                Debug.WriteLine("Device is now active. ts: {0}", now);
            }
            else
            {
                _connectStateHolder.PutState.IsActive = false;
                _connectStateHolder.PutState.StartedPlayingAt = 0L;
            }
        }

        private void SendReply(string key, RequestResult result)
        {
            var success = result == RequestResult.Success;
            var reply =
                $"{{\"type\":\"reply\", \"key\": \"{key.ToLower()}\", \"payload\": {{\"success\": {success.ToString().ToLowerInvariant()}}}}}";
            Debug.WriteLine(reply);
            _client.Send(reply);
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

                    return new SpotifyWebsocketRequest(mid, pid, sender, (JObject) command, key);
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
            ConState?.Dispose();
            _pingTokenSource?.Dispose();
            _linkedToken?.Dispose();
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
            PutHttpClient?.Dispose();
            WaitForConnectionId?.Dispose();
        }

        public async Task<AckResponse> SendCommandToDevice(string activeDeviceDeviceId, object p1,
            CancellationToken ct = default)
        {
            var spclient = await ApResolver.GetClosestSpClient();
            var resp = 
                await spclient
                .AppendPathSegments("connect-state", "v1", "player", "command", "from", AudioOutput.DeviceId, "to",
                    activeDeviceDeviceId)
                .WithOAuthBearerToken((await ConState.GetToken(ct)).AccessToken)
                .PostJsonAsync(p1, cancellationToken: ct);
            return await resp.GetJsonAsync<AckResponse>();
        }

        public async Task<AckResponse> SendTransferCommand(string activeDeviceDeviceId,
            CancellationToken ct = default)
        {
            var spclient = await ApResolver.GetClosestSpClient();
            var resp =
                await spclient
                    .AppendPathSegments("connect-state", "v1", "connect", "transfer", "from", AudioOutput.DeviceId,
                        "to",
                        activeDeviceDeviceId)
                    .WithOAuthBearerToken((await ConState.GetToken(ct)).AccessToken)
                    .PostJsonAsync(new
                    {
                        transfer_options = new
                        {
                            restore_paused = "restore"
                        }
                    }, cancellationToken: ct);
            return await resp.GetJsonAsync<AckResponse>();
        }

        public readonly struct AckResponse
        {
            [Newtonsoft.Json.JsonConstructor]
            public AckResponse(string ackId)
            {
                AckId = ackId;
            }
            [JsonPropertyName("ack_id")]
            public string AckId { get; }
        }

    }
}