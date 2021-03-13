using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using Spotify;
using Spotify.Player.Proto.Transfer;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Contexts;
using SpotifyLibV2.Connect.Events;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Models;
using SpotifyLibV2.Models.Public;
using SpotifyLibV2.Models.Request;

namespace SpotifyLibV2.Connect
{
    public class SpotifyConnectState : ISpotifyConnectState, IMessageListener, IRequestListener
    {
        private readonly DeviceInfo _deviceInfo;
        private volatile string _connectionId = null;
        private readonly ISpotifyConnectReceiver _receiver;
        private readonly PutStateRequest _putState;
        private readonly SpotifyConfiguration _config;
        private long _previousSet;
        private bool? _previousPause;
        private bool? _previousShuffle;
        private RepeatState? _previousRepeatState;
        private PlayingChangedRequest _currentCluster;
        private readonly ITokensProvider _tokens;
        private HttpClient _putclient;
        private readonly ISpotifyPlayer _player;
        private readonly APWelcome _apWelcome;
        private readonly IEventsService _events;

        private readonly LocalStateWrapper _stateWrapper;
        public SpotifyConnectState(
            IDealerClient dealerClient,
            ISpotifyPlayer device,
            ISpotifyConnectReceiver receiver,
            ITokensProvider tokens,
            SpotifyConfiguration config,
            ConcurrentDictionary<string, string> attributes,
            uint initialVolume,
            int volumeSteps, APWelcome apWelcome, IEventsService events)
        {
            _previousPause = null;
            _receiver = receiver;
            _player = device;
            _config = config;
            _apWelcome = apWelcome;
            _events = events;
            _tokens = tokens;
            _stateWrapper = new LocalStateWrapper(device, config, attributes, apWelcome);

            this._deviceInfo = InitializeDeviceInfo(initialVolume, volumeSteps);
            this._putState = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Connectstate.Device
                {
                    DeviceInfo = _deviceInfo
                }
            };
            this.ConnectState = InitState();
            dealerClient.AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
            dealerClient.AddRequestListener(this,
                "hm://connect-state/v1/");
        }
        public string ActiveDeviceId { get; set; }
        public PlayerState ConnectState { get; }
        public async Task<byte[]> UpdateState(PutStateReason reason, int playerTime, PlayerState state)
        {
            if (_connectionId == null) throw new ArgumentNullException(_connectionId);

            if (playerTime == -1) _putState.HasBeenPlayingForMs = 0L;
            else _putState.HasBeenPlayingForMs = (ulong)playerTime;

            _putState.PutStateReason = reason;
            _putState.ClientSideTimestamp = (ulong)TimeProvider.CurrentTimeMillis();
            _putState.Device.DeviceInfo = _deviceInfo;
            _putState.Device.PlayerState = state;
            return await PutConnectState(_putState);
        }

        private IEnumerable<string> _previousDevices;
        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            switch (uri)
            {
                case { } str when str.StartsWith("hm://track-playback/v1/command"):
                    Debugger.Break();
                    break;
                case { } str when str.StartsWith("hm://pusher/v1/connections/"):
                    UpdateConnectionId(headers["Spotify-Connection-Id"]);
                    break;
                case { } str when str.Equals("hm://connect-state/v1/connect/volume"):
                    var cmd = SetVolumeCommand.Parser.ParseFrom(payload);
                    lock (this)
                    {
                        _deviceInfo.Volume = (uint)cmd.Volume;
                        if (cmd.CommandOptions != null)
                        {
                            _putState.LastCommandMessageId = ((uint)cmd.CommandOptions.MessageId);
                            _putState.LastCommandSentByDeviceId = string.Empty;
                        }
                    }

                    Debug.WriteLine("Update volume. volume: {0}/{1}", cmd.Volume, 100);
                    _receiver.VolumeChanged(cmd.Volume);
                    break;
                case { } str when str.StartsWith("hm://connect-state/v1/cluster"):
                    var update = ClusterUpdate.Parser.ParseFrom(payload);

                    if (!string.IsNullOrEmpty(update.DevicesThatChanged.FirstOrDefault()))
                    {
                        switch (update.UpdateReason)
                        {
                            case ClusterUpdateReason.DeviceStateChanged:
                                if (ActiveDeviceId != update.Cluster.ActiveDeviceId)
                                {
                                    //update new device?
                                    _receiver.DeviceChanged(update.DevicesThatChanged.ToArray(),
                                        SpotDeviceAction.DeviceFocusGot);
                                    if (!string.IsNullOrEmpty(ActiveDeviceId))
                                        _receiver.DeviceChanged(new[] { ActiveDeviceId }, SpotDeviceAction.DeviceFocusLost);
                                }
                                else
                                {
                                    //do nothing?
                                }

                                break;
                            case ClusterUpdateReason.DevicesDisappeared:
                                _receiver.DeviceChanged(update.DevicesThatChanged.ToArray(),
                                    SpotDeviceAction.DevicesDisappeared);
                                break;
                            case ClusterUpdateReason.NewDeviceAppeared:
                                _receiver.DeviceChanged(update.DevicesThatChanged.ToArray(),
                                    SpotDeviceAction.NewDeviceAppeared);
                                break;
                        }
                    }

                    Task.Run(() => _receiver.NewItem(UpdateFromCluster(update.Cluster)));
                    break;
                case { }:
                    Debug.WriteLine("Message left unhandled! uri: {0}", uri);
                    break;
            }
            return Task.FromResult("done");
        }

        private PlayingChangedRequest UpdateFromCluster(Cluster cluster)
        {
            if (_previousPause != cluster.PlayerState.IsPaused)
            {
                _previousPause = cluster.PlayerState.IsPaused;
                Task.Run(() =>
                    _receiver.PauseChanged(cluster.PlayerState.IsPaused));
            }

            var now = TimeProvider.CurrentTimeMillis();
            Debug.WriteLine("Received cluster update at {0}: {1}", now, cluster);
            var ts = cluster.Timestamp - 3000; // Workaround

            try
            {
                if (!_config.DeviceId.Equals(cluster.ActiveDeviceId)
                    && _putState.IsActive
                    && (ulong)now > _putState.StartedPlayingAt
                    && (ulong)ts > _putState.StartedPlayingAt)
                {

                }
            }
            catch (Exception)
            {
                // ignored
            }

            var updated = cluster.ActiveDeviceId != ActiveDeviceId;
            if (updated)
            {
                ActiveDeviceId = cluster.ActiveDeviceId;
                //_receiver.DeviceChanged(ActiveDeviceId);
            }
            long timeStamp = 0;
            //if (update?.Cluster?.PlayerState?.PositionAsOfTimestamp != _previousSet)
            //  {
            if (cluster.PlayerState != null)
            {
                _previousSet = cluster.PlayerState.PositionAsOfTimestamp;
                var diff = (int)(TimeProvider.CurrentTimeMillis() - cluster.PlayerState.Timestamp);
                Debug.WriteLine("Expected timestamp: " +
                                (int)(cluster.PlayerState.PositionAsOfTimestamp + diff));
                Task.Run(() => _receiver.PositionChanged(cluster.PlayerState.PositionAsOfTimestamp + diff));
                timeStamp = cluster.PlayerState.PositionAsOfTimestamp + diff;
            }
            //   }

            if (cluster.PlayerState?.Options != null)
            {
                if (cluster.PlayerState.Options.ShufflingContext != _previousShuffle)
                {
                    Task.Run(() => _receiver.ShuffleStateChanged(cluster.PlayerState.Options.ShufflingContext));
                    _previousShuffle = cluster.PlayerState.Options.ShufflingContext;
                }

                var repeatingTrack = cluster.PlayerState.Options.RepeatingTrack;
                var repeatingContext = cluster.PlayerState.Options.RepeatingContext;
                if (repeatingContext && !repeatingTrack)
                {
                    _previousRepeatState = RepeatState.Context;
                    Task.Run(() => _receiver.RepeatStateChanged(RepeatState.Context));
                }
                else
                {
                    if (repeatingTrack)
                    {
                        _previousRepeatState = RepeatState.Track;
                        Task.Run(() => _receiver.RepeatStateChanged(RepeatState.Track));
                    }
                    else
                    {
                        _previousRepeatState = RepeatState.Off;
                        Task.Run(() => _receiver.RepeatStateChanged(RepeatState.Off));
                    }
                }
            }

            if (_currentCluster.ItemUri != cluster.PlayerState?.Track?.Uri)
            {
                var j = new PlayingChangedRequest(
                    _previousRepeatState ?? RepeatState.Off,
                    _previousShuffle ?? false,
                    cluster.PlayerState?.Track?.Uri,
                    cluster.PlayerState.ContextUri,
                    cluster.PlayerState?.IsPaused,
                    !cluster.PlayerState?.IsPaused,
                    timeStamp, null
                );
                _currentCluster = j;
                // Task.Run(() => _receiver.NewItem(j));
            }

            return _currentCluster;
        }

        private void UpdateConnectionId([NotNull] string conId)
        {
            try
            {
                conId = HttpUtility.UrlDecode(conId, Encoding.UTF8);
            }
            catch (Exception)
            {
                //ignored
            }

            if (_connectionId != null && _connectionId.Equals(conId)) return;
            _connectionId = conId;
            Debug.WriteLine("Updated Spotify-Connection-Id: " + _connectionId);
            ConnectState.IsSystemInitiated = true;
            var data = UpdateState(PutStateReason.NewDevice, -1, ConnectState)
                .ContinueWith(z =>
                {
                    var tryGet =
                        Connectstate.Cluster.Parser.ParseFrom(z.Result);
                    var j = UpdateFromCluster(tryGet);
                    _receiver.Ready(j);
                });
        }

        private DeviceInfo InitializeDeviceInfo(uint initialVolume, int volumeSteps)
            => new()
            {
                CanPlay = true,
                Volume = initialVolume,
                Name = _config.DeviceName,
                DeviceId = _config.DeviceId,
                DeviceType = _config.DeviceType,
                DeviceSoftwareVersion = "Spotify-11.1.0",
                SpircVersion = "3.2.6",
                Capabilities = new Capabilities
                {
                    CanBePlayer = true,
                    GaiaEqConnectId = true,
                    SupportsLogout = true,
                    IsObservable = true,
                    CommandAcks = true,
                    SupportsRename = true,
                    SupportsTransferCommand = true,
                    SupportsCommandRequest = true,
                    VolumeSteps = volumeSteps,
                    SupportsGzipPushes = true,
                    NeedsFullPlayerState = true,
                    SupportedTypes =
                    {
                        new List<string>
                        {
                            {"audio/episode"},
                            {"audio/track"}
                        }
                    }
                }
            };
        private static PlayerState InitState()
        {
            return new()
            {
                PlaybackSpeed = 1.0,
                SessionId = string.Empty,
                PlaybackId = string.Empty,
                ContextRestrictions = new Connectstate.Restrictions(),
                Options = new ContextPlayerOptions
                {
                    RepeatingContext = false,
                    ShufflingContext = false,
                    RepeatingTrack = false
                },
                PositionAsOfTimestamp = 0,
                Position = 0,
                IsPlaying = false
            };
        }
        private async Task<byte[]> PutConnectState([NotNull] PutStateRequest req)
        {
            try
            {
                _putclient ??= new HttpClient
                {
                    BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
                };
                _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    _tokens.GetToken("playlist-read").AccessToken);
                var byteArrayContent = new ByteArrayContent(req.ToByteArray());
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
                _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);
                //    _putclient.DefaultRequestHeaders.Add("Accept", "application/json");
                var res = await _putclient.PutAsync($"/connect-state/v1/devices/{_config.DeviceId}", byteArrayContent);

                if (res.IsSuccessStatusCode)
                {
                    var dt = await res.Content.ReadAsByteArrayAsync();
                    Debug.WriteLine("Put new connect state:");
                    return dt;
                }
                else
                {
                    Debugger.Break();
                    return new byte[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed updating state.", ex); return new byte[0];
            }
        }

        public RequestResult OnRequest(string mid, int pid, string sender, JObject command)
        {
            _putState.LastCommandMessageId = (uint) pid;
            _putState.LastCommandSentByDeviceId = sender;

            var enumParsed = (command["endpoint"]?.ToString()).StringToEndPoint();
            if (enumParsed != Endpoint.Error)
            {
                NotifyCommand(enumParsed, new CommandBody(command));
                return RequestResult.Success;
            }
            return RequestResult.UnknownSendCommandResult;
        }


        private async Task HandleTransfer(TransferState transferData)
        {
            Debug.WriteLine($"Loading Context : uri {transferData.CurrentSession.Context.Uri}");

            try
            {
           //     var sessionId = _stateWrapper.Transfer(transferData); //TODO
               // _events.OnContextChanged() //not relevant
               //await LoadSession(sessionId, )
            }
            catch (Exception x)
            {
                switch (x)
                {
                    case IOException _:
                    case MercuryException _:
                        Debug.WriteLine($"Failed loading context {x}");
                        //Somehow notify the UI applications(receivers)
                        _receiver.ErrorOccured(x);
                        break;
                    case UnsupportedContextException unsupportedContext:
                        //User probably tried to play a local track. We want to support this in the feauture
                        //so for now we'll just notify the receiver about a local playback but still as an error.
                        _receiver.PlayLocalTrack();
                        _receiver.ErrorOccured(x);
                        break;
                }
            }
        }

        private Task LoadSession(string sessionId)
        {
            Debug.WriteLine($"Loading session, id {sessionId}");

            //var transitionInfo = TransitionInfo

          //  _playerSession = new _playerSession();
            _events.SendEvent(new NewSessionIdEvent(sessionId, _stateWrapper).BuildEvent());

            return LoadTrack();
        }

        private async Task LoadTrack()
        {
            Debug.WriteLine($"Loading track id: {_stateWrapper.GetPlayableItem}");
        }
        private void NotifyCommand([NotNull] Endpoint endpoint, [NotNull] CommandBody data)
        {
            if (_player == null)
            {
                Debug.WriteLine("Cannot dispatch command because there are no listeners. command: {0}", endpoint);
                return;
            }

            switch (endpoint)
            {
                case Endpoint.Play:
                    break;
                case Endpoint.Pause:
                    break;
                case Endpoint.Resume:
                    break;
                case Endpoint.SeekTo:
                    break;
                case Endpoint.SkipNext:
                    break;
                case Endpoint.SkipPrev:
                    break;
                case Endpoint.SetShufflingContext:
                    break;
                case Endpoint.SetRepeatingContext:
                    break;
                case Endpoint.SetRepeatingTrack:
                    break;
                case Endpoint.UpdateContext:
                    break;
                case Endpoint.SetQueue:
                    break;
                case Endpoint.AddToQueue:
                    break;
                case Endpoint.Transfer:
                    Task.Run(async () =>
                    {
                        await HandleTransfer(TransferState.Parser.ParseFrom(data.Data));
                    });
                    break;
                case Endpoint.Error:
                    throw new InvalidCommandException(endpoint, data);
                default:
                    //Should never hit
                    throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null);
            }
        }
    }

    public class InvalidCommandException : Exception
    {
        public InvalidCommandException(Endpoint endpoint, CommandBody payload) : base(
            $"Invalid command received: {endpoint}; Payload: {System.Convert.ToBase64String(payload.Data)}")
        {
            Endpoint = endpoint;
            Payload = System.Convert.ToBase64String(payload.Data);
        }

        public string Payload { get; }
        public Endpoint Endpoint { get; }
    }
}
