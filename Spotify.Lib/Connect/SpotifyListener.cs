using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Connectstate;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Org.BouncyCastle.Utilities.Encoders;
using Spotify.Lib.Connect.Audio;
using Spotify.Lib.Connect.Contexts;
using Spotify.Lib.Connect.DataHolders;
using Spotify.Lib.Exceptions;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;
using Spotify.Lib.Models.Ids;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using static Spotify.Lib.Connect.DataHolders.TracksKeeperHelpers;
namespace Spotify.Lib.Connect
{
    public readonly struct SessionHolder
    {
        public readonly QueueNode<AbsChunkedStream> Head;
        internal readonly PlayerSessionHolder Session;
        public readonly int LastPosition;
        internal SessionHolder(QueueNode<AbsChunkedStream> head, PlayerSessionHolder session,
            int lastpos)
        {
            Head = head;
            Session = session;
            LastPosition = lastpos;
        }
    }
    internal readonly struct CommandBody
    {
        internal CommandBody(JObject obj)
        {
            Obj = obj;

            Data = obj.ContainsKey("data") ? Base64.Decode(obj["data"].ToString()) : null;

            Value = obj.ContainsKey("value") ? obj["value"].ToString() : null;
        }

        public JObject Obj { get; }

        public byte[] Data { get; }

        public string Value { get; }


        public int? ValueInt()
        {
            return Value == null ? null : int.Parse(Value);
        }

        public bool? ValueBool()
        {
            return Value == null ? null : bool.Parse(Value);
        }
    }

    internal class SpotifyRequestListener : IRequestListener, IPlayerSessionListener
    {
        public readonly string ConnectionId;
        internal SpotifyRequestListener(string connectionId)
        {
            ConnectionId = connectionId;
            _newPutstate = NewPutState();
            var cl = new LoggingHandler(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            }, SpotifyClient.Instance.Tokens);
            _putclient = new HttpClient(cl)
            {
                BaseAddress = new Uri(AsyncContext.Run(async () => await ApResolver.GetClosestSpClient()))
            };
            _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", ConnectionId);

            _playerstate =
                InitState(new PlayerState());
            DealerClient.Instance.AddRequestListener(this, "hm://connect-state/v1/");



            async void StateChanged(MediaPlaybackState state, TrackOrEpisode? args)
            {
                _newPutstate.Device.PlayerState = _playerstate;
                switch (state)
                {
                    case MediaPlaybackState.VolumeChanged:
                        //normalized so 0 to 1..
                        // volume/max = norm
                        //volume = norm * max
                        var volume = SpotifyConnectReceiver.Instance.Player.NormalizedVolume * SpotifyConnectReceiver.Instance.Player.VolumeMax;
                        _newPutstate.Device.DeviceInfo.Volume = (uint)volume;
                        await UpdateState(PutStateReason.PlayerStateChanged, GetPosition(_playerstate), _newPutstate);
                        break;
                    case MediaPlaybackState.None:
                        break;
                    case MediaPlaybackState.Buffering:
                        break;
                    //case MediaPlaybackState.FinishedLoading:
                    //    FinishedLoading(playerstate, args.Value, ref value);
                    //    break;
                    case MediaPlaybackState.TrackStarted:
                        SetState(_playerstate, true, false, false);
                        await UpdateState(PutStateReason.PlayerStateChanged, GetPosition(_playerstate), _newPutstate);
                        break;
                    case MediaPlaybackState.TrackPaused:
                        SetState(_playerstate, true, true, false);
                        await UpdateState(PutStateReason.PlayerStateChanged, GetPosition(_playerstate), _newPutstate);
                        break;
                    case MediaPlaybackState.TrackPlayed:
                        break;
                    case MediaPlaybackState.NewTrack:
                        break;
                    case MediaPlaybackState.PositionChanged:
                        SetPosition(_playerstate, (int)SpotifyConnectReceiver.Instance.Player.Position.TotalMilliseconds);
                        await UpdateState(PutStateReason.PlayerStateChanged, GetPosition(_playerstate), _newPutstate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }

            SpotifyConnectReceiver.Instance.PlayerChangedInternal += (sender, tuple) =>
            {
                if (tuple.Old != null)
                    tuple.Old.StateChanged -= StateChanged;

                if (tuple.New != null)
                    tuple.New.StateChanged += StateChanged;
            };

            if (SpotifyConnectReceiver.Instance.Player != null)
            {
                SpotifyConnectReceiver.Instance.Player.StateChanged += StateChanged;
            }
        }


        private readonly PlayerState _playerstate;
        private readonly PutStateRequest _newPutstate;
        private readonly HttpClient _putclient;
        public async Task<RequestResult> OnRequest(string mid, int pid, string sender, JObject command)
        {
            _newPutstate.LastCommandMessageId = (uint)pid;
            _newPutstate.LastCommandSentByDeviceId = sender;

            if (SpotifyConnectReceiver.Instance.Player == null)
            {
                return RequestResult.DeviceDoesNotSupportCommand;
            }
            var cmd = new CommandBody(command);
            var enumParsed = (command["endpoint"]?.ToString())?.StringToEndPoint();
            switch (enumParsed)
            {
                case Endpoint.Transfer:
                    HandleTransfer(_newPutstate,
                        _playerstate,
                        TransferState.Parser.ParseFrom(cmd.Data), 
                        SpotifyConnectReceiver.Instance.Player,
                        this,
                        this);
                    return RequestResult.Success;
                case Endpoint.Play:
                    HandlePlay(cmd.Obj,
                        _newPutstate,
                        _playerstate,
                        SpotifyConnectReceiver.Instance.Player,
                        this,
                        this);
                    return RequestResult.Success;
                case Endpoint.Error:
                    return RequestResult.UpstreamError;
                default:
                    return RequestResult.DeviceDoesNotSupportCommand;
            }
        }

        

        private static void HandlePlay(JObject obj, PutStateRequest putState,
            PlayerState playerState,
            ISpotifyPlayer instancePlayer,
            IPlayerSessionListener listener, 
            SpotifyRequestListener instance)
        {
            Debug.WriteLine("Loading context (play), uri: {}", PlayCommandHelper.GetContextUri(obj));

            try
            {
                playerState.PlayOrigin = ProtoUtils.JsonToPlayOrigin(PlayCommandHelper.GetPlayOrigin(obj));
                playerState.Options = ProtoUtils.JsonToPlayerOptions(PlayCommandHelper.GetPlayerOptionsOverride(obj),
                    playerState.Options);
                TracksKeeper? tracksKeeper = null;
                var session = SetContext(putState, playerState, 
                    ProtoUtils.JsonToContext(PlayCommandHelper.GetContext(obj)), ref tracksKeeper);

                tracksKeeper = new TracksKeeper(session.Context);

                var trackUid = PlayCommandHelper.GetSkipToUid(obj);
                var trackUri = PlayCommandHelper.GetSkipToUri(obj);
                var trackIndex = PlayCommandHelper.GetSkipToIndex(obj);
                var value = tracksKeeper!.Value;
                var pagesCopy = session.Pages; //copy itno new field
                try
                {
                    if (trackUri != null && !trackUri.IsEmpty())
                    {
                        InitializeFrom(ref value,
                            ref pagesCopy, playerState, session.Context, list =>
                            {
                                return list.FindIndex(a => a.Uri == trackUri);
                            }, null, null);
                    }
                    else if (trackUid != null && !trackUid.IsEmpty())
                    {
                        InitializeFrom(ref value,
                            ref pagesCopy, playerState, session.Context, list =>
                            {
                                return list.FindIndex(a => a.Uid == trackUid);
                            }, null, null);
                    }
                    else if (trackIndex != null)
                    {
                        InitializeFrom(ref value,
                            ref pagesCopy, playerState, session.Context, list =>
                            {
                                return trackIndex < list.Count
                                    ? trackIndex.Value
                                    : -1;
                            }, null, null);
                    }
                    else
                    {
                        InitializeStart(ref value, session.Context, playerState,ref pagesCopy);
                    }
                }
                catch (IllegalStateException ex)
                {
                    Debug.WriteLine("Failed initializing tracks, falling back to start. uri: {0}, uid: {1}, index: {2}", trackUri, trackUid, trackIndex);
                    InitializeStart(ref value, session.Context, playerState, ref pagesCopy);
                }

                var seekTo = PlayCommandHelper.GetSeekTo(obj);
                SetPosition(playerState, seekTo ?? 0);

                //LoadTransforming();
                //return sessionId;
                var paused = PlayCommandHelper.IsInitiallyPaused(obj);

                if (paused == null) paused = false;
                var stream = AsyncContext.Run(() => SpotifyConnectReceiver.Instance
                    .Player.TryFetchStream(PlayableId.From(playerState.Track)));
                FinishedLoading(playerState, stream.TrackOrEpisode.Value, ref value);

                CurrentSession =
                    LoadSession(stream, session.SessionId,
                    !paused.Value,
                    true, playerState,
                    instancePlayer,
                    listener);
                SpotifyConnectReceiver.Instance.OnNewPlaybackWrapper(null, playerState);
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

        public SessionHolder HandlePlayInternal(
            int durationMs,
            AbsChunkedStream stream,
            IPlayRequest context,
            int? seekTo = null,
            ISpotifyPlayer? playerOverride = null)
        {
            var playerState = _playerstate;
            var putState = _newPutstate;
            playerState.PlayOrigin = new Connectstate.PlayOrigin
            {
                FeatureIdentifier = "harmony",
                FeatureVersion = "4.13.0-6c0e4f7"
            };

            playerState.Options = new ContextPlayerOptions
            {
                RepeatingContext = context.RepeatContext ?? false,
                RepeatingTrack = context.RepeatTrack ?? false,
                ShufflingContext = context.Shuffle ?? false
            };

            var ctx = new Context
            {
                Uri = context.ContextUri,
                Restrictions = new Spotify.Player.Proto.Restrictions(),
                Loading = false,
            };
            ctx.Metadata.Add("zelda.context_uri", context.ContextUri);

            switch (context)
            {
                case ContextRequest contextRequest:
                    var resolvedPages = AsyncContext.Run(() =>
                        SpotifyClient.Instance.SendAsyncReturnJson<MercuryContextWrapperResponse>(RawMercuryRequest.Get(
                            $"hm://context-resolve/v1/{contextRequest.ContextUri}"), CancellationToken.None, true));
                    var str = resolvedPages.Pages;
                    var pages = ProtoUtils.JsonToContextPages((JArray)str
                                                                ?? throw new InvalidOperationException());
                    ctx.Pages.AddRange(pages);
                    break;
                case PagedRequest paged:
                    var newPage = new ContextPage
                    {
                        Loading = false,
                        PageUrl = string.Empty,
                        NextPageUrl = string.Empty
                    };
                    newPage.Tracks.AddRange(paged.Uris.Select(a => new ContextTrack
                    {
                        Uri = a,
                        Gid = ByteString.CopyFrom(Utils.HexToBytes(PlayableId.FromUri(a).ToHexId()))
                    }));
                    ctx.Pages.Add(newPage);
                    break;
            }

            TracksKeeper? tracksKeeper = null;
            var session = SetContext(putState, playerState,
                ctx, ref tracksKeeper);

            tracksKeeper = new TracksKeeper(session.Context);


            var value = tracksKeeper!.Value;
            var pagesCopy = session.Pages; //copy itno new field

            var contextData = SetContext(putState, playerState, ctx, ref tracksKeeper);

            InitializeFrom(ref value, ref pagesCopy, playerState, contextData.Context,
                list =>
            {
                if (context.PlayIndex < list.Count) return (int)context.PlayIndex;
                return -1;
            }, null, null);

            UpdateTrackDuration(ref value, playerState, durationMs);
            playerState.PositionAsOfTimestamp = 0;

            playerState.Timestamp = TimeProvider.CurrentTimeMillis();

            if (seekTo != null) SetPosition(playerState, (long)seekTo);
            else SetPosition(playerState, 0);

            CurrentSession =
                LoadSession(stream, session.SessionId,
                    true,
                    true, playerState,
                    playerOverride ?? SpotifyConnectReceiver.Instance.Player, 
                    this);
            return CurrentSession;
        }

        public static IPlayerSessionListener Instance { get; private set; }

        #region Request/Device
        private static void HandleTransfer(PutStateRequest putState,
            PlayerState playerstate, TransferState transferData,
            ISpotifyPlayer player,
            IPlayerSessionListener listener,
            SpotifyRequestListener instance)
        {
            Debug.WriteLine($"Loading Context : uri {transferData.CurrentSession.Context.Uri}");

            try
            {
                var ps = transferData.CurrentSession;
                playerstate.PlayOrigin =
                    ProtoUtils.ConvertPlayOrigin(ps.PlayOrigin);
                playerstate.Options =
                    ProtoUtils.ConvertPlayerOptions(transferData.Options);

                TracksKeeper? tracksKeeper = null;
                var session = SetContext(putState, playerstate, ps.Context, 
                    ref tracksKeeper);
                var pb = transferData.Playback;

                tracksKeeper = new TracksKeeper(session.Context);
                var value = tracksKeeper!.Value;
                var pagesCopy = session.Pages; //copy itno new field
                try
                {
                    InitializeFrom(ref value, ref pagesCopy, playerstate, session.Context, 
                        list =>
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var track = list[i];
                            if ((track.HasUid && ps.CurrentUid.Equals(track.Uid)) ||
                                ProtoUtils.TrackEquals(track, pb.CurrentTrack))
                            {
                                return i;
                            }
                        }

                        return -1;
                    }, pb.CurrentTrack, transferData.Queue);
                }
                catch (IllegalStateException ex)
                {
                    Debug.WriteLine("Failed initializing tracks, falling back to start. uid: {0}", ps.CurrentUid);
                    InitializeStart(ref value, session.Context, playerstate, ref pagesCopy);
                }

                playerstate.PositionAsOfTimestamp = pb.PositionAsOfTimestamp;

                if (pb.IsPaused) playerstate.Timestamp = TimeProvider.CurrentTimeMillis();
                else playerstate.Timestamp = pb.Timestamp;

                var waitForEverything = new ManualResetEvent(false);

                var stream = AsyncContext.Run(() => SpotifyConnectReceiver.Instance
                    .Player.TryFetchStream(PlayableId.From(playerstate.Track)));
                FinishedLoading(playerstate, stream.TrackOrEpisode.Value, ref value);

                CurrentSession = 
                    LoadSession(stream, session.SessionId, 
                    !transferData.Playback.IsPaused, 
                    true,playerstate, 
                    player,
                    listener);
                waitForEverything.Set();
                SpotifyConnectReceiver.Instance.OnNewPlaybackWrapper(null, playerstate);
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
        public static SessionHolder CurrentSession { get; private set; }
        private static SessionHolder LoadSession(AbsChunkedStream stream, string sessionId,
            bool play,
            bool withSkip,
            PlayerState state,
            ISpotifyPlayer player,
            IPlayerSessionListener listener)
        {
            Debug.WriteLine($"Loading session, id {sessionId}");

            var transitionInfo = 
                TransitionInfo.ContextChange(withSkip, GetPosition(state));

            var newSessionHolder = new PlayerSessionHolder(sessionId, listener); 
            var res = LoadTrack(stream, play, transitionInfo, player, state, ref newSessionHolder);

           return new SessionHolder(res.head, newSessionHolder, res.lastPlayPos);
        }

      
        private static (QueueNode<AbsChunkedStream>? head, int lastPlayPos) LoadTrack(
            AbsChunkedStream stream, 
            bool willPlay, 
            TransitionInfo transitionInfo,
            ISpotifyPlayer player,
            PlayerState state,
            ref PlayerSessionHolder sessionHolder)
        {
            Debug.WriteLine($"Loading track id: {PlayableId.From(state.Track)}");

            var playbackId = SessionHelper.LoadSession(ref sessionHolder, player, stream,
                GetPosition(state), transitionInfo.StartedReason);
            state.PlaybackId = playbackId.head.Item.PlaybackId;

            //  if (willPlay) ConnectClient.Player.Resume(_stateWrapper.GetPosition());
            //else ConnectClient.Player.Pause();

            return playbackId;
        }

        public static int GetPosition(PlayerState state)
        {
            int diff = (int)(TimeProvider.CurrentTimeMillis() - state.Timestamp);
            return (int)(state.PositionAsOfTimestamp + diff);
        }
        public static void FinishedLoading(PlayerState state,
            TrackOrEpisode metadata,
            ref TracksKeeper trackskeeper)
        {
            EnrichWithMetadata(state, metadata, ref trackskeeper);
            SetBuffering(state, false);
        }
        internal static void SetBuffering(PlayerState state, bool b)
        {
            SetState(state, true, state.IsPaused, b);
        }
        private static void SetState(
            PlayerState playerstate,
            bool playing, 
            bool paused,
            bool buffering)
        {
            if (paused && !playing) throw new ArgumentNullException();
            else if (buffering && !playing) throw new ArgumentNullException();

            var wasPaused = IsPaused(playerstate);
            playerstate.IsPlaying = playing;
            playerstate.IsPaused = paused;
            playerstate.IsBuffering = buffering;

            if (wasPaused && !paused) // Assume the position was set immediately before pausing
                SetPosition(playerstate, playerstate.PositionAsOfTimestamp);
        }
        internal static void SetPosition(PlayerState state, long pos)
        {
            state.Timestamp = TimeProvider.CurrentTimeMillis();
            state.PositionAsOfTimestamp = pos;
            state.Position = 0L;
        }
        public static bool IsPaused(PlayerState state) => state.IsPlaying && state.IsPaused;

        public void NotActive()
        {
            throw new NotImplementedException();
        }
        internal static void EnrichWithMetadata(PlayerState state,
            TrackOrEpisode metadata,
            ref TracksKeeper tracksKeeper)
        {
            if (metadata.track == null) throw new IllegalStateException();
            var track = metadata.track;
            if (track.HasDuration) UpdateTrackDuration(ref tracksKeeper, state, track.Duration);

            var builder = state.Track;
            if (track.HasPopularity) builder.Metadata["popularity"] = track.Popularity.ToString();
            if (track.HasExplicit) builder.Metadata["is_explicit"] = track.Explicit.ToString();
            if (track.HasHasLyrics) builder.Metadata["has_lyrics"] = track.HasHasLyrics.ToString();
            if (track.HasName) builder.Metadata["title"] = track.Name;
            if (track.HasDiscNumber)
                builder.Metadata["album_disc_number"] =
                    track.DiscNumber.ToString();

            for (int i = 0; i < track.Artist.Count; i++)
            {
                var artist = track.Artist[i];
                if (artist.HasName) builder.Metadata["artist_name" + (i == 0 ? "" : (":" + i))] =
                     artist.Name;
                if (artist.HasGid) builder.Metadata["artist_uri" + (i == 0 ? "" : (":" + i))] = 
                        ArtistId.FromHex(artist.Gid.ToByteArray().BytesToHex()).Uri;
            }

            if (track.Album != null)
            {
                var album = track.Album;
                if (album.Disc.Count > 0)
                {
                    builder.Metadata["album_track_count"]=  ProtoUtils.GetTrackCount(album).ToString();
                    builder.Metadata["album_disc_count"] = album.Disc.Count.ToString();
                }

                if (album.HasName) builder.Metadata["album_title"] = album.Name;
                if (album.HasGid)
                    builder.Metadata["album_uri"] =
                        AlbumId.FromHex(album.Gid.ToByteArray().BytesToHex()).Uri;

                for (int i = 0; i < album.Artist.Count; i++)
                {
                    var artist = album.Artist[i];
                    if (artist.HasName)
                        builder.Metadata["album_artist_name" + (i == 0 ? "" : (":" + i))] = 
                            artist.Name;
                    if (artist.HasGid)
                        builder.Metadata["album_artist_uri" + (i == 0 ? "" : (":" + i))] =
                            ArtistId.FromHex(artist.Gid.ToByteArray().BytesToHex()).Uri;
                }

                if (track.HasDiscNumber)
                {
                    foreach (var disc in album.Disc)
                    {
                        if (disc.Number != track.DiscNumber) continue;

                        for (int i = 0; i < disc.Track.Count; i++)
                        {
                            if (disc.Track[i].Gid.Equals(track.Gid))
                            {
                                builder.Metadata["album_track_number"] =
                                    (i + 1).ToString();
                                break;
                            }
                        }
                    }
                }

                if (album.CoverGroup?.Image != null) 
                    ImageId.PutAsMetadata(builder, album.CoverGroup);
            }

            ProtoUtils.PutFilesAsMetadata(builder, track.File.ToList());
            state.Track = builder;
        }


        internal readonly struct ContextData
        {
            public readonly AbsSpotifyContext Context;
            public readonly string SessionId;
            public readonly Pages Pages;

            public ContextData(AbsSpotifyContext context, string sessionId, Pages pages)
            {
                Context = context;
                SessionId = sessionId;
                Pages = pages;
            }
        }
        private static ContextData SetContext(
            PutStateRequest putState,
            PlayerState playerState,
            Player.Proto.Context ctx,
            ref TracksKeeper? trackskeeper)
        {
            var uri = ctx.Uri;
            var context = AbsSpotifyContext.From(uri);
            playerState.ContextUri = uri;

            if (!context.IsFinite())
            {
                SetRepeatingContext(context,
                    playerState,
                    false);
                SetShufflingContext(
                    ref trackskeeper,
                    context,
                    playerState,
                    false);
            }

            if (ctx.HasUrl) playerState.ContextUrl = ctx.Url;
            else playerState.ContextUrl = string.Empty;

            playerState.ContextMetadata.Clear();

            ProtoUtils.CopyOverMetadata(ctx, playerState);

            var pages = Pages.From(ctx);

            SetIsActive(putState, true);

            return new ContextData(context, RenewSessionId(playerState), pages);
        }
        public static void SetShufflingContext(
            ref TracksKeeper? tracksKeeper,
            AbsSpotifyContext context,
            PlayerState playerState, bool value)
        {
            if (context == null || tracksKeeper == null) return;

            var old = IsShufflingContext(playerState);
            playerState.Options.ShufflingContext =
                value && context.Restrictions.Can(RestrictionsManager.Action.SHUFFLE);

            if (old != IsShufflingContext(playerState))
                ToggleShuffle(tracksKeeper, IsShufflingContext(playerState));
        }

        public static void SetRepeatingContext(
            AbsSpotifyContext context,
            PlayerState playerState,
            bool value)
        {
            if (context == null) return;

            playerState.Options.RepeatingContext =
                true && context.Restrictions.Can(RestrictionsManager.Action.REPEAT_CONTEXT);
        }
        public static void SetIsActive(PutStateRequest request, bool b)
        {
            if (b)
            {
                if (!request.IsActive)
                {
                    long now = TimeProvider.CurrentTimeMillis();
                    request.IsActive = true;
                    request.StartedPlayingAt = (ulong)now;
                    Debug.WriteLine("Device is now active. ts: {0}", now);
                }
            }
            else
            {
                request.IsActive = false;
                request.StartedPlayingAt = 0L;
            }
        }
        public static bool IsShufflingContext(PlayerState state) => state.Options.ShufflingContext;
        internal async Task<byte[]> UpdateState(
            PutStateReason reason,
            int playerTime,
            PutStateRequest putStateRequest)
        {
            if (playerTime == -1) putStateRequest.HasBeenPlayingForMs = 0L;
            else putStateRequest.HasBeenPlayingForMs = (ulong)playerTime;

            putStateRequest.PutStateReason = reason;
            putStateRequest.ClientSideTimestamp = (ulong)TimeProvider.CurrentTimeMillis();
            return await PutConnectState(putStateRequest);
        }
        private async Task<byte[]> PutConnectState(PutStateRequest incomingPutRequest)
        {
            try
            {

                var asBytes = incomingPutRequest.ToByteArray();

                using var ms = new MemoryStream();
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(asBytes, 0, asBytes.Length);
                }
                ms.Position = 0;
                var content = new StreamContent(ms);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
                content.Headers.ContentEncoding.Add("gzip");


                var res = await 
                    _putclient.
                        PutAsync($"/connect-state/v1/devices/{SpotifyConfig.DeviceId}",
                            content);
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
        #endregion
        private static string Base64UrlEncode(byte[] inputBytes)
        {
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-') // replace URL unsafe characters with safe ones
                .Replace('/', '_') // replace URL unsafe characters with safe ones
                .Replace("=", ""); // no padding
        }
        private static string GenerateSessionId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            var str = Base64UrlEncode(bytes);
            return str;
        }
        private static string RenewSessionId(PlayerState state)
        {
            var sessionId = GenerateSessionId();
            state.SessionId = sessionId;
            return sessionId;
        }
        private static PutStateRequest NewPutState()
        {
            return new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Device
                {
                    DeviceInfo = InitializeDeviceInfo()
                }
            };
        }
        private static DeviceInfo InitializeDeviceInfo()
        {
            return new()
            {
                CanPlay = true,
                Volume = 65536,
                Name = SpotifyConfig.DeviceName,
                DeviceId = SpotifyConfig.DeviceId,
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
            };
        }
        private static PlayerState InitState(PlayerState plstate)
        {
            plstate.PlaybackSpeed = 1.0;
            plstate.SessionId = string.Empty;
            plstate.PlaybackId = string.Empty;
            plstate.Suppressions = new Suppressions();
            plstate.ContextRestrictions = new Connectstate.Restrictions();
            plstate.Options = new ContextPlayerOptions
            {
                RepeatingTrack = false,
                ShufflingContext = false,
                RepeatingContext = false
            };
            plstate.Position = 0;
            plstate.PositionAsOfTimestamp = 0;
            plstate.IsPlaying = false;
            return plstate;
        }

        public async Task Initialize()
        {
            Debug.WriteLine("Updated Spotify-Connection-Id: " + ConnectionId);
            _playerstate.IsSystemInitiated = true;
            _newPutstate.Device.PlayerState = _playerstate;
            var data = await
                UpdateState(PutStateReason.NewDevice, -1, _newPutstate);

            var tryGet =
                Connectstate.Cluster.Parser.ParseFrom(data);
            SpotifyConnectReceiver.Instance.OnNewPlaybackWrapper(null, tryGet);
        }
        public void FinishedLoading(PlayerSessionHolder session, TrackOrEpisode metadata)
        {
            throw new NotImplementedException();
        }
    }

    internal class SpotifyMessageListener : IMessageListener
    {
        internal SpotifyMessageListener()
        {
            PreviousCluster = null;
            _currentPosition = 0;
            DealerClient.Instance.AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
        }
        #region Messages

        internal Cluster PreviousCluster { get; set; }
        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            if (uri.StartsWith("hm://pusher/v1/connections/"))
            {
                return SpotifyConnectReceiver.Instance.
                    UpdateConnectionId(headers["Spotify-Connection-Id"]);
            }
            else if (uri.StartsWith("hm://connect-state/v1/cluster"))
            {
                var update = ClusterUpdate.Parser.ParseFrom(payload);
                if (update.Cluster?.PlayerState != null)
                {
                    SpotifyConnectReceiver.Instance.OnQueueUpdate(update.Cluster.PlayerState.NextTracks);
                    if (update.Cluster.PlayerState?.Track?.Uri != null)
                        if (PreviousCluster?.PlayerState?.Track?.Uri
                            != update.Cluster.PlayerState.Track.Uri)
                            SpotifyConnectReceiver.Instance.OnNewPlaybackWrapper(this, update.Cluster.PlayerState);
                    SpotifyConnectReceiver.Instance.OnPlaybackStateChanged(this,
                        update.Cluster.PlayerState.IsPaused);

                    SpotifyConnectReceiver.Instance.OnShuffleStatecHanged(this,
                        update.Cluster!.PlayerState!.Options.ShufflingContext);
                    SpotifyConnectReceiver.Instance.OnRepeatStateChanged(this, ParseRepeatState(update.Cluster));


                    if (Math.Abs(_currentPosition - GetPosition(update.Cluster)) > 10)
                    {
                        var curPos = GetPosition(update.Cluster);
                        SpotifyConnectReceiver.Instance.OnPositionChanged(this, curPos);
                        _currentPosition = curPos;
                    }
                }
                var now = TimeProvider.CurrentTimeMillis();
                var ts = update.Cluster.Timestamp - 3000; // Workaround
                //if (_requestState != null)
                //    if (!SpotifyConfig.DeviceId.Equals(update.Cluster.ActiveDeviceId)
                //        && _requestState.IsActive
                //        && now > (long)_requestState.PutStateRequest.StartedPlayingAt
                //        && ts > (long)_requestState.PutStateRequest.StartedPlayingAt)
                //        foreach (var dealerClientReqListener in _dealerClient.ReqListeners)
                //            dealerClientReqListener.Value.NotActive();

                if (update?.Cluster != null)
                    SpotifyConnectReceiver.Instance.NotifyDeviceUpdates(this, update.Cluster);


                PreviousCluster = update.Cluster;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
        private double _currentPosition;
        internal int GetPosition(Cluster cluster)
        {
            var diff = (int)(TimeProvider.CurrentTimeMillis() - cluster.PlayerState.Timestamp);
            return (int)(cluster.PlayerState.PositionAsOfTimestamp + diff);
        }

        private static RepeatState ParseRepeatState(Cluster cluster)
        {
            var repeatingTrack = cluster.PlayerState.Options.RepeatingTrack;
            var repeatingContext = cluster.PlayerState.Options.RepeatingContext;
            if (repeatingContext && !repeatingTrack) return RepeatState.Context;

            if (repeatingTrack)
                return RepeatState.Track;
            return RepeatState.Off;
        }
        #endregion
    }
}
