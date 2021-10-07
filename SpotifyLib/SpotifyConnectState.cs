using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Connectstate;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Spotify.Player.Proto;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Models;
using SpotifyLib.Models.Contexts;
using SpotifyLib.Models.Player;
using SpotifyLib.Models.Response;
using SpotifyLib.Models.Transitions;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using Restrictions = Connectstate.Restrictions;

namespace SpotifyLib
{
    public class SpotifyConnectState
    {
        internal readonly SpotifyWebsocketState WsState;
        internal PutStateRequest PutState { get; private set; }

        public SpotifyConnectState(SpotifyWebsocketState wsState)
        {
            WsState = wsState;
            WsState.AudioOutput.AudioOutputStateChanged += AudioOutputOnAudioOutputStateChanged; 
            PutState = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Device
                {
                    DeviceInfo = new DeviceInfo()
                    {
                        CanPlay = true,
                        Volume = 65536,
                        Name = WsState.ConState.Config.DeviceName,
                        DeviceId = WsState.ConState.Config.DeviceId,
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
                    }
                }
            };
            State = InitState();
        }


        private static PlayerState InitState(PlayerState playerState = null)
        {
            if (playerState != null)
            {
                playerState.PlaybackSpeed = 1.0;
                playerState.SessionId = string.Empty;
                playerState.PlaybackId = string.Empty;
                playerState.Suppressions = new Suppressions();
                playerState.ContextRestrictions = new Restrictions();
                playerState.Options = new ContextPlayerOptions
                {
                    RepeatingTrack = false,
                    ShufflingContext = false,
                    RepeatingContext = false
                };
                playerState.Position = 0;
                playerState.PositionAsOfTimestamp = 0;
                playerState.IsPlaying = false;
                playerState.IsSystemInitiated = true;
                return playerState;
            }
            return new PlayerState
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
            };
        }
        AsyncLock m = new AsyncLock();
        internal async Task<byte[]> UpdateState(
            PutStateReason reason, PlayerState st,
            int playertime = -1,
            bool @break = false)
        {
            using (await m.LockAsync())
            {
                var timestamp = (ulong)TimeHelper.CurrentTimeMillisSystem;
                if (playertime == -1)
                    PutState.HasBeenPlayingForMs = 0L;
                else
                    PutState.HasBeenPlayingForMs = (ulong)Math.Min((ulong)playertime,
                        timestamp - PutState.StartedPlayingAt);

                PutState.PutStateReason = reason;
                PutState.ClientSideTimestamp = timestamp;
                PutState.Device.PlayerState = st;
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

                var res = await WsState.PutHttpClient
                    .PutAsync($"/connect-state/v1/devices/{WsState.ConState.Config.DeviceId}", content, CancellationToken.None);
                if (res.IsSuccessStatusCode)
                {
                    //if (@break) Debugger.Break();
                    return await res.Content.ReadAsByteArrayAsync();
                }

                throw new HttpRequestException(res.StatusCode.ToString());
            }
        }

        private async void AudioOutputOnAudioOutputStateChanged(object sender, AudioOutputStateChanged e)
        {
            await Task.Delay(100);
            switch (e)
            {
                case AudioOutputStateChanged.Buffering:
                    {
                        if (!State.IsPaused)
                        {
                            SetState(true, State.IsPaused, true);
                            var updated = await UpdateState(
                                PutStateReason.PlayerStateChanged,
                                State,
                                WsState
                                .AudioOutput
                                .Position);
                        }
                    }
                    break;
                case AudioOutputStateChanged.ManualSeek:
                {
                    SetPosition(WsState.AudioOutput.Position);
                    var updated = await UpdateState(PutStateReason.PlayerStateChanged,
                        State,
                        WsState.AudioOutput.Position, true);
                }
                    break;
                case AudioOutputStateChanged.Playing:
                    {
                        SetState(true, false, false);
                        var updated = await UpdateState(PutStateReason.PlayerStateChanged,
                            State,
                            WsState.AudioOutput.Position, true);
                    }
                    break;
                case AudioOutputStateChanged.Paused:
                    {
                        SetState(true, true, false);
                        var updated = await UpdateState(PutStateReason.PlayerStateChanged, State, WsState.AudioOutput
                          .Position);
                    }
                    break;
                case AudioOutputStateChanged.Finished:
                    break;
            }
        }

        public void NotActive()
        {
            PutState.IsActive = false;
            PutState.PutStateReason = 0;
            PutState.MessageId = 0;
            PutState.LastCommandMessageId = 0;
            PutState.LastCommandSentByDeviceId = "";
            PutState.StartedPlayingAt = 0L;
            PutState.HasBeenPlayingForMs = 0L;
            PutState.ClientSideTimestamp = 0L;
            InitState(State);

            WsState.SetDeviceIsActive(false);
            WsState.AudioOutput.Stop();
            UpdateState(PutStateReason.BecameInactive, State, (int) WsState.AudioOutput.Position);
            Debug.WriteLine("Notified inactivity!");
        }

        public void SetState(bool playing, bool paused, bool buffering)
        {
            if (paused && !playing) throw new IllegalStateException();
            else if (buffering && !playing) throw new IllegalStateException();

            var wasPaused = IsPaused();
            State.IsPlaying = playing;
            State.IsPaused = paused;
            State.IsBuffering = buffering;

            if (wasPaused && !paused) // Assume the position was set immediately before pausing
                SetPosition(State.PositionAsOfTimestamp);
        }
        public void SetPosition(long pos)
        {
            State.Timestamp = TimeHelper.CurrentTimeMillisSystem;
            State.PositionAsOfTimestamp = pos;
            State.Position = 0L;
        }
        public bool IsPaused() => State.IsPlaying && State.IsPaused;

        internal AbsSpotifyContext Context;
        internal PlayerState State { get; }
        internal string SetContext(Context ctx)
        {
            var uri = ctx.Uri;
            Context = AbsSpotifyContext.From(uri);
            State.ContextUri = uri;

            if (!Context.IsFinite)
            {
                SetRepeatingContext(false);
                SetRepeatingContext(false);
            }

            State.ContextUrl = ctx.HasUrl ? ctx.Url : string.Empty;

            State.ContextMetadata.Clear();
            ProtoUtils.CopyOverMetadata(ctx, State);

            Pages = PagesLoader.From(WsState.ConState, ctx);
            TracksKeeper = new TracksKeeper(this);

            WsState.SetDeviceIsActive(true);

            return RenewSessionId();
        }

        internal async Task LoadSession(string sessionId, bool play, bool withSkip)
        {
            Debug.WriteLine($"Loading session, id {sessionId}");

            var transitionInfo = TransitionInfo.ContextChange(this,
                withSkip);

            if (Session != null)
            {
                Session.FinishedLoading -= SessionOnFinishedLoading;
                Session.Dispose();
            }

            Session = new PlayerSession(WsState,
                sessionId);
            Session.FinishedLoading += SessionOnFinishedLoading;
            //_events.SendEvent(new NewSessionIdEvent(sessionId, _stateWrapper).BuildEvent());

            await LoadTrack(play, transitionInfo);
        }

        private async void SessionOnFinishedLoading(object sender, ChunkedStream e)
        {
            EnrichWithMetadata(e);
            SetState(true, State.IsPaused, false);
            await UpdateState(PutStateReason.PlayerStateChanged, State, WsState.AudioOutput
                .Position);
        }

        private async Task LoadTrack(bool willPlay, TransitionInfo transitionInfo)
        {
            Debug.WriteLine($"Loading track id: {GetCurrentPlayable.Uri}");
            var playbackId = await Session.Load(GetCurrentPlayableOrThrow,
                (int)State.Position,
                transitionInfo.StartedReason);
            State.PlaybackId = playbackId.PlaybackId;
            if (willPlay)
                WsState.AudioOutput.Resume(GetPosition());
            else
                WsState.AudioOutput.Pause();


        }
        public long GetPosition()
        {
            int diff = (int)(TimeHelper.CurrentTimeMillisSystem - State.Timestamp);
            return (int)(State.PositionAsOfTimestamp + diff);
        }
        public PlayerSession Session { get; set; }
        public TracksKeeper TracksKeeper { get; set; }
        public PagesLoader Pages { get; set; }

        public SpotifyId GetCurrentPlayableOrThrow
        {
            get
            {
                var id = GetCurrentPlayable;
                if (id.Uri == null)
                    throw new IllegalStateException();
                return id;
            }
        }

        public SpotifyId GetCurrentPlayable => TracksKeeper == null
            ? new SpotifyId()
            : PlayableId.From(State.Track);
        public bool IsRepeatingContext => State.Options.RepeatingContext;
        public bool IsShufflingContext => State.Options.ShufflingContext;
        private void SetRepeatingContext(bool value)
        {
            if (Context == null) return;

            State.Options.RepeatingContext =
                value && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_CONTEXT);
        }
        private string RenewSessionId()
        {
            var sessionId = GenerateSessionId();
            State.SessionId = sessionId;
            return sessionId;
        }

        private static string GenerateSessionId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            var str = Base64UrlEncode(bytes);
            return str;
            //var str = Convert.ToBase64String(bytes).Replace("-", "");
            //str = str.Replace("=", "");
            //return str;
        }
        public static string GeneratePlaybackId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            bytes[0] = 1;
            return bytes.BytesToHex().ToLowerInvariant();
        }

        private static string Base64UrlEncode(byte[] inputBytes)
        {
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-') // replace URL unsafe characters with safe ones
                .Replace('/', '_') // replace URL unsafe characters with safe ones
                .Replace("=", ""); // no padding
        }
        private void EnrichWithMetadata(ChunkedStream metadata)
        {
            if (metadata.TrackOrEpisode.Id.Type == AudioItemType.Track)
            {
                var track = metadata.TrackOrEpisode.Track;
                if (State.Track == null) throw new NotImplementedException();

                if (track.HasDuration) TracksKeeper.UpdateTrackDuration(track.Duration);

                var b = State.Track;

                if (track.HasPopularity) b.Metadata["popularity"] = track.Popularity.ToString();
                if (track.HasExplicit) b.Metadata["is_explicit"] = track.Explicit.ToString().ToLower();
                if (track.HasName) b.Metadata["title"] = track.Name;
                if (track.HasDiscNumber) b.Metadata["album_disc_number"] = track.DiscNumber.ToString();
                for (var i = 0; i < track.Artist.Count; i++)
                {
                    var artist = track.Artist[i];
                    if (artist.HasName) b.Metadata["artist_name" + (i == 0 ? "" : (":" + i))] = artist.Name;
                    if (artist.HasGid)
                        b.Metadata["artist_uri" + (i == 0 ? "" : (":" + i))] =
                            SpotifyId.FromHex(artist.Gid.ToByteArray().BytesToHex(), AudioItemType.Artist).Uri;
                }

                if (track.Album != null)
                {
                    var album = track.Album;
                    if (album.Disc.Count > 0)
                    {
                        b.Metadata["album_track_count"] = album.Disc.Select(z => z.Track).Count().ToString();
                        b.Metadata["album_disc_count"] = album.Disc.Count.ToString();
                    }

                    if (album.HasName) b.Metadata["album_title"] = album.Name;
                    if (album.HasGid)
                        b.Metadata["album_uri"] =
                            SpotifyId.FromHex(album.Gid.ToByteArray().BytesToHex(), AudioItemType.Album).Uri;

                    for (int i = 0; i < album.Artist.Count; i++)
                    {
                        var artist = album.Artist[i];
                        if (artist.HasName)
                            b.Metadata["album_artist_name" + (i == 0 ? "" : (":" + i))] = artist.Name;
                        if (artist.HasGid)
                            b.Metadata["album_artist_uri" + (i == 0 ? "" : (":" + i))] =
                                SpotifyId.FromHex(artist.Gid.ToByteArray().BytesToHex(), AudioItemType.Artist).Uri;
                    }

                    if (track.HasDiscNumber)
                    {
                        b.Metadata["album_track_number"] =
                            album.Disc.SelectMany(z => z.Track).ToList().FindIndex(k => k.Gid == track.Gid) +
                            1.ToString();
                    }

                    if (album.CoverGroup?.Image != null
                        && album.CoverGroup.Image.Count > 0)
                        ImageId.PutAsMetadata(b, album.CoverGroup);
                }


                var k = new JArray();
                foreach (var j in track.File
                    .Where(z => z.HasFormat))
                {
                    k.Add(j.Format.ToString());
                }

                b.Metadata["available_file_formats"] = k.ToString();
                State.Track = b;
            }
        }
    }
}
