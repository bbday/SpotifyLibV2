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
using Extensions;
using Google.Protobuf;
using MediaLibrary.Enums;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto.Transfer;
using Spotify.Playlist4.Proto;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Audio.PlayerSessions;
using SpotifyLibrary.Audio.Transitions;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyRequestState : IRequestListener, IPlayerSessionListener
    {
        private HttpClient _putclient;
        private string _connectionId;
        private readonly ISpotifyLibrary _library;
        private readonly DeviceInfo _deviceInfo;
        internal readonly SpotifyConnectReceiver ConnectClient;
        private readonly LocalStateWrapper _stateWrapper;
        private PlayerSession _playerSession;

        internal SpotifyRequestState(ISpotifyLibrary library,
            DealerClient dealerClient,
            SpotifyConnectReceiver spotifyConnectReceiver)
        {
            _library = library;
            _deviceInfo = InitializeDeviceInfo();
            ConnectClient = spotifyConnectReceiver;
            PutStateRequest = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Connectstate.Device
                {
                    DeviceInfo = _deviceInfo
                }
            };
            
            _stateWrapper = new LocalStateWrapper(this, _library.MercuryClient);
            dealerClient.AddRequestListener(this,
                "hm://connect-state/v1/");
        }
        public PutStateRequest PutStateRequest { get; }

        public void UpdateVolumeInDeviceInfo(double normalizedVolume)
        {
            _deviceInfo.Volume = (uint) normalizedVolume;
        }
        public async Task<RequestResult> OnRequest(string mid, int pid, string sender, JObject command)
        {
            PutStateRequest.LastCommandMessageId = (uint)pid;
            PutStateRequest.LastCommandSentByDeviceId = sender;
            IsActive = true;
            var cmd = new CommandBody(command);
            var enumParsed = (command["endpoint"]?.ToString())?.StringToEndPoint();
            switch (enumParsed)
            {
                case Endpoint.Play:
                    await HandlePlay(command);
                    ConnectClient.OnPlaybackStateChanged(this, false);
                    return RequestResult.Success;
                case Endpoint.Pause:
                    _stateWrapper.SetPosition(_stateWrapper.GetPosition());
                    ConnectClient.Player.Pause();
                    ConnectClient.OnPlaybackStateChanged(this, true);
                    return RequestResult.Success;
                case Endpoint.Resume:
                    ConnectClient.Player.
                        Resume(-1);
                    ConnectClient.OnPlaybackStateChanged(this, false);
                    return RequestResult.Success;
                case Endpoint.SeekTo:
                    var pos = command["value"]!.ToObject<int>();
                    _stateWrapper.SetPosition(pos);
                    ConnectClient.Player.Seek(this, pos);
                    ConnectClient.OnPositionChanged(this, pos);
                    return RequestResult.Success;
                case Endpoint.Transfer:
                    ConnectClient.OnIncomingTransfer(this);
                    try
                    {
                        await HandleTransfer(TransferState.Parser.ParseFrom(cmd.Data));
                        ConnectClient.OnTransferdone(this, null);
                        return RequestResult.Success;
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine(x.ToString());
                        ConnectClient.OnTransferdone(this, x);
                        return RequestResult.UpstreamError;
                    }
                case Endpoint.Error:
                    return RequestResult.UnknownSendCommandResult;
                default:
                    return RequestResult.DeviceDoesNotSupportCommand;
            }
        }

        public async Task HandlePlay(JObject obj)
        {
            Debug.WriteLine($"Loading context (play), uri: {PlayCommandHelper.GetContextUri(obj)}");

            try
            {
                var sessionId = await _stateWrapper.Load(obj);

                var paused = PlayCommandHelper.IsInitiallyPaused(obj) ?? false;

                var stream = await ConnectClient.Player.TryFetchStream(_stateWrapper.GetPlayableItem);
                FinishedLoading(stream.TrackOrEpisode.Value);

                await LoadSession(stream, sessionId, !paused, PlayCommandHelper.WillSkipToSomething(obj));
                ConnectClient.OnNewPlaybackWrapper(this, _stateWrapper.PlayerState);
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

                //get stream

                var stream = await ConnectClient.Player.TryFetchStream(_stateWrapper.GetPlayableItem);
                FinishedLoading(stream.TrackOrEpisode.Value);
                await LoadSession(stream, sessionId, !transferData.Playback.IsPaused, true);
                ConnectClient.OnNewPlaybackWrapper(this, _stateWrapper.PlayerState);
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

        internal async Task<AbsChunkedStream> HandlePlayInternal(
            int durationMs,
            AbsChunkedStream stream,
            global::SpotifyLibrary.Models.Requests.PagedRequest context,
            byte[] audioKey)
        {
            var sessionId = await
                _stateWrapper.LoadOnDevice(durationMs,context, 0);

            await LoadSession(stream, sessionId, false, true);
            return stream;
        }
        private async Task LoadSession(AbsChunkedStream stream, string sessionId,
            bool play,
            bool withSkip)
        {
            Debug.WriteLine($"Loading session, id {sessionId}");

            var transitionInfo = TransitionInfo.ContextChange(_stateWrapper, withSkip);

            _playerSession = new PlayerSession(_library,
                ConnectClient.Player,
                sessionId, this);

            await LoadTrack(stream, play, transitionInfo);
            ConnectClient.OnNewPlaybackWrapper(this, _stateWrapper.PlayerState);
            //_events.SendEvent(new NewSessionIdEvent(sessionId, _stateWrapper).BuildEvent());

        }
        private async Task LoadTrack(AbsChunkedStream stream, bool willPlay, TransitionInfo transitionInfo)
        {
            Debug.WriteLine($"Loading track id: {_stateWrapper.GetPlayableItem}");

            var playbackId = await _playerSession.Load(stream,
                _stateWrapper.GetPosition(),
                transitionInfo.StartedReason);

            _stateWrapper.SetPlaybackId(playbackId);
            //_events.SendEvent(new NewPlaybackIdEvent(_stateWrapper.SessionId, playbackId).BuildEvent());

            //_stateWrapper.SetState(true, !willPlay, true);

            //await _stateWrapper.Updated();

            if (willPlay) ConnectClient.Player.Resume(_stateWrapper.GetPosition());
            else ConnectClient.Player.Pause();
        }

        public bool IsActive { get; private set; }

        public async void NotActive()
        {
            var pos = (int) ConnectClient.Player.Position.TotalMilliseconds;
            ConnectClient.Player.Inactive();
            IsActive = false;
            var st =
                _stateWrapper.PlayerState;
            st.Timestamp = 0L;
            st.ContextUri = string.Empty;
            st.ContextUrl = string.Empty;

            st.ContextRestrictions = null;
            st.PlayOrigin = null;
            st.Index = null;
            st.Track = null;
            st.PlaybackId = string.Empty;
            st.PlaybackSpeed = 0D;
            st.PositionAsOfTimestamp = 0L;
            st.Duration = 0L;
            st.IsPlaying = false;
            st.IsPaused = false;
            st.IsBuffering = false;
            st.IsSystemInitiated = false;

            st.Options = null;
            st.Restrictions = null;
            st.Suppressions = null;
            st.PrevTracks.Clear();
            st.NextTracks.Clear();
            st.ContextMetadata.Clear();
            st.PageMetadata.Clear();
            st.SessionId = string.Empty;
            st.QueueRevision = string.Empty;
            st.Position = 0L;
            st.EntityUri = string.Empty;

            st.Reverse.Clear();
            st.Future.Clear();

            LocalStateWrapper.InitState(st);
            SetIsActive(false);
            await UpdateState(PutStateReason.BecameInactive, pos);

            Debug.WriteLine($"Notified inactivity");
        }
        internal async Task UpdateConnectionId(string conId)
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

            _putclient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            })
            {
                BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
            };
            _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                (await _library.Tokens.GetToken("playlist-read")).AccessToken);
            _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);



            Debug.WriteLine("Updated Spotify-Connection-Id: " + _connectionId);
            _stateWrapper.PlayerState.IsSystemInitiated = true;
            var data = await
                UpdateState(PutStateReason.NewDevice, -1);

            var tryGet =
                Connectstate.Cluster.Parser.ParseFrom(data);
            ConnectClient.OnNewCluster(tryGet);
            if (tryGet?.PlayerState?.Track != null)
                ConnectClient.OnNewPlaybackWrapper(this, tryGet);
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
        private async Task<byte[]> PutConnectState(PutStateRequest incomingPutRequest)
        {
            try
            {

                var asBytes = incomingPutRequest.ToByteArray();
                if (_putclient == null)
                {
                    _putclient = new HttpClient(new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                    })
                    {
                        BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
                    };
                    _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        (await _library.Tokens.GetToken("playlist-read")).AccessToken);
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


                var res = await _putclient.PutAsync($"/connect-state/v1/devices/{_library.Configuration.DeviceId}", content);
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
                Name = _library.Configuration.DeviceName,
                DeviceId = _library.Configuration.DeviceId,
                DeviceType = _library.Configuration.DeviceType,
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

        public void FinishedLoading(TrackOrEpisode metadata)
        {
            _stateWrapper.EnrichWithMetadata(metadata);
            _stateWrapper.SetBuffering(false);
        }
    }
}
