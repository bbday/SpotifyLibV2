using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Math.EC;
using Spotify.Player.Proto.Transfer;
using SpotifyLibrary.Connect.Enums;
using SpotifyLibrary.Connect.Exceptions;
using SpotifyLibrary.Connect.Helpers;
using SpotifyLibrary.Connect.Player;
using SpotifyLibrary.Connect.PlayerSession;
using SpotifyLibrary.Connect.Transitions;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Player;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyRequestState : IRequestListener, IPlayerSessionListener
    {
        private string _connectionId;
        private HttpClient _putclient;
        private readonly DeviceInfo _deviceInfo;
        private readonly LocalStateWrapper _stateWrapper;

        private readonly DealerClient _dealerClient;
        public readonly SpotifyConnectClient ConnectClient;
        private PlayerSession.PlayerSession _playerSession;
        private ICdnManager _cdnManager => ConnectClient.Client.CdnManager;

        internal SpotifyRequestState(
            DealerClient dealerClient,
            SpotifyConnectClient spotifyConnectClient)
        {
            ConnectClient = spotifyConnectClient;
            _dealerClient = dealerClient;

            _deviceInfo = InitializeDeviceInfo();
            PutStateRequest = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Connectstate.Device
                {
                    DeviceInfo = _deviceInfo
                }
            };
            _stateWrapper = new LocalStateWrapper(this);
            dealerClient.AddRequestListener(this,
                "hm://connect-state/v1/");
        }

        public RequestResult OnRequest(string mid,
            int pid,
            string sender,
            JObject command)
        {
            PutStateRequest.LastCommandMessageId = (uint) pid;
            PutStateRequest.LastCommandSentByDeviceId = sender;
            var cmd = new CommandBody(command);
            var enumParsed = (command["endpoint"]?.ToString()).StringToEndPoint();
            switch (enumParsed)
            {
                case Endpoint.Play:
                    _ = HandlePlay(command);
                    break;
                case Endpoint.Pause:
                    _stateWrapper.SetPosition(_stateWrapper.Position);
                    Player.Pause();
                    break;
                case Endpoint.Resume:
                    Player.Resume(false, -1);
                    break;
                case Endpoint.SeekTo:
                    var pos = command["value"].ToObject<int>();
                    _stateWrapper.SetPosition(pos);
                    Player.Seek(pos);
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
                    _ =  HandleTransfer(TransferState.Parser.ParseFrom(cmd.Data));
                    break;
                case Endpoint.Error:
                default:
                    return RequestResult.UnknownSendCommandResult;
            }

            return RequestResult.Success;
        }

        private async Task HandlePlay(JObject obj)
        {
            Debug.WriteLine($"Loading context (play), uri: {PlayCommandHelper.GetContextUri(obj)}");

            try
            {
                var sessionId = await _stateWrapper.Load(obj);

                var paused = PlayCommandHelper.IsInitiallyPaused(obj) ?? false;
                await LoadSession(sessionId, !paused, PlayCommandHelper.WillSkipToSomething(obj));
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case IOException:
                    case MercuryException:
                        Debug.WriteLine($"Failed loading context! {ex.ToString()}");
                        break;
                    case UnsupportedContextException unsup:
                        Debug.WriteLine($"Cannot play local tracks!! {ex.ToString()}");
                        break;
                }
            }
        }

        private async Task HandleTransfer(TransferState transferData)
        {
            Debug.WriteLine($"Loading Context : uri {transferData.CurrentSession.Context.Uri}");
            try
            {
                var sessionId = await
                    _stateWrapper.Transfer(transferData); 

                await LoadSession(sessionId, !transferData.Playback.IsPaused, true);
            }
            catch (Exception x)
            {
                switch (x)
                {
                    case IOException _:
                    case MercuryException _:
                        Debug.WriteLine($"Failed loading context {x}");
                        //Somehow notify the UI applications(receivers)
                        throw new NotImplementedException();
                        //_receiver.ErrorOccured(x);
                        break;
                    case UnsupportedContextException unsupportedContext:
                        //User probably tried to play a local track. We want to support this in the feauture
                        //so for now we'll just notify the receiver about a local playback but still as an error.
                        throw new NotImplementedException();
                        //_receiver.PlayLocalTrack();
                        // _receiver.ErrorOccured(x);
                        break;
                }
            }
        }

        private async Task LoadSession(string sessionId, bool play, bool withSkip)
        {
            Debug.WriteLine($"Loading session, id {sessionId}");

            var transitionInfo = TransitionInfo.ContextChange(_stateWrapper, withSkip);

            _playerSession = new PlayerSession.PlayerSession(Player, ConnectClient.Client.Config, 
                sessionId, this, 
                _cdnManager);
            //_events.SendEvent(new NewSessionIdEvent(sessionId, _stateWrapper).BuildEvent());

            await LoadTrack(play, transitionInfo);
        }

        private async Task LoadTrack(bool willPlay, TransitionInfo transitionInfo)
        {
            Debug.WriteLine($"Loading track id: {_stateWrapper.GetPlayableItem}");
            var playbackId = await _playerSession.Load(_stateWrapper.GetPlayableItem,
                _stateWrapper.Position,
                transitionInfo.StartedReason);

            _stateWrapper.SetPlaybackId(playbackId);
            //_events.SendEvent(new NewPlaybackIdEvent(_stateWrapper.SessionId, playbackId).BuildEvent());

            //_stateWrapper.SetState(true, !willPlay, true);

            //await _stateWrapper.Updated();

             if (willPlay) Player.Resume(true, _stateWrapper.Position);
            else Player.Pause();
        }

        public PutStateRequest PutStateRequest { get; }

        internal async Task UpdateConnectionId([NotNull] string conId)
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
            _stateWrapper.PlayerState.IsSystemInitiated = true;
            var data = await 
                UpdateState(PutStateReason.NewDevice, -1);

            var tryGet =
                Connectstate.Cluster.Parser.ParseFrom(data);

            ConnectClient.OnNewPlaybackWrapper(tryGet);
        }

        internal async Task<byte[]> UpdateState(PutStateReason reason, int playerTime)
        {
            if (_connectionId == null) throw new ArgumentNullException(_connectionId);

            if (playerTime == -1) PutStateRequest.HasBeenPlayingForMs = 0L;
            else PutStateRequest.HasBeenPlayingForMs = (ulong)playerTime;

            PutStateRequest.PutStateReason = reason;
            PutStateRequest.ClientSideTimestamp = (ulong)TimeProvider.CurrentTimeMillis();
            PutStateRequest.Device.DeviceInfo = _deviceInfo;
            PutStateRequest.Device.PlayerState = _stateWrapper.PlayerState;
            return await PutConnectState(PutStateRequest);
        }

        private async Task<byte[]> PutConnectState([NotNull] IMessage incomingPutRequest)
        {
            try
            {

                var asBytes = incomingPutRequest.ToByteArray();
                if (_putclient == null)
                {
                    _putclient = new HttpClient(new HttpClientHandler()
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    })
                    {
                        BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
                    };
                    _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        (await Client.Tokens.GetToken("playlist-read")).AccessToken);
                    _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);
                }

                using var ms = new MemoryStream();
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(asBytes, 0, asBytes.Length);
                }
                ms.Position = 0;
                var content = new StreamContent(ms);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
                content.Headers.ContentEncoding.Add("gzip");


                var res = await _putclient.PutAsync($"/connect-state/v1/devices/{Client.Config.DeviceId}", content);
                if (res.IsSuccessStatusCode)
                {
                    var dt = await res.Content.ReadAsByteArrayAsync();
                    Debug.WriteLine("Put new connect state:");
                    return dt;
                }
                else
                {
                    Debugger.Break();
                    //TODO: error handling
                    return new byte[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed updating state.", ex);
                return new byte[0];
            }
        }

        private DeviceInfo InitializeDeviceInfo()
        {
            return new DeviceInfo
            {
                CanPlay = true,
                Volume = 65536,
                Name = Client.Config.DeviceName,
                DeviceId = Client.Config.DeviceId,
                DeviceType = Client.Config.DeviceType,
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
            };
        }
        private SpotifyClient Client => ConnectClient.Client;
        public ISpotifyPlayer Player => ConnectClient.Player;


        public ISpotifyId CurrentPlayable() => _stateWrapper.GetPlayableItem;

        public ISpotifyId NextPlayable()
        {
            throw new NotImplementedException();
        }

        public ISpotifyId NextPlayableDoNotSet()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> MetadataFor(ISpotifyId playable) => _stateWrapper.MetadataFor(playable);


        public async void PlaybackHalted(int chunk)
        {
            Debug.WriteLine("Playback halted on retrieving chunk {0}.", chunk);
            _stateWrapper.SetBuffering(true);
          //  await _stateWrapper.Updated();
        }

        public async void PlaybackResumedFromHalt(int chunk, long diff)
        {
            Debug.WriteLine("Playback resumed, chunk {0} retrieved, took {1}ms.", chunk, diff);
            _stateWrapper.SetPosition(_stateWrapper.Position - diff);
            _stateWrapper.SetBuffering(true);
           // await _stateWrapper.Updated();
        }

        public async void StartedLoading()
        {
            if (!_stateWrapper.IsPaused)
            {
                _stateWrapper.SetBuffering(true);
                await _stateWrapper.Updated();
            }
        }


        public void LoadingError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public async void FinishedLoading(TrackOrEpisode metadata)
        {
            _stateWrapper.EnrichWithMetadata(metadata);
            _stateWrapper.SetBuffering(false);
          //  await _stateWrapper.Updated(false);
        }

        public void PlaybackError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void TrackChanged(string playbackId, TrackOrEpisode metadata, int pos, TransitionReason startedReason)
        {
            throw new NotImplementedException();
        }

        public void TrackPlayed(string playbackId, TransitionReason endReason, int endedAt)
        {
            throw new NotImplementedException();
        }

        public void SetIsActive(bool b)
        {
            if (b)
            {
                if (!PutStateRequest.IsActive)
                {
                    long now = TimeProvider.CurrentTimeMillis();
                    PutStateRequest.IsActive = true;
                    PutStateRequest.StartedPlayingAt = (ulong)now;
                    Debug.WriteLine("Device is now active. ts: {0}", now);
                }
            }
            else
            {
                PutStateRequest.IsActive = false;
                PutStateRequest.StartedPlayingAt = 0L;
            }
        }
    }
}
