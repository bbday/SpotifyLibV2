using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;
using SpotifyLib.Models;
using SpotifyLib.Models.Contexts;
using SpotifyLib.Models.Player;
using SpotifyLib.Models.Response;
using SpotifyLib.Models.Transitions;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;

namespace SpotifyLib
{
    public class SpotifyConnectState
    {
        internal readonly SpotifyWebsocketState WsState;
        public SpotifyConnectState(SpotifyWebsocketState wsState)
        {
            WsState = wsState;
            WsState.AudioOutput.AudioOutputStateChanged += (sender, changed) =>
            {
                switch (changed)
                {
                    case AudioOutputStateChanged.Buffering:
                        if (!State.IsPaused)
                        {
                            State.IsBuffering = true;
                            _ = wsState.UpdateState(PutStateReason.PlayerStateChanged, wsState.AudioOutput
                                .Position);
                        }
                        break;
                    case AudioOutputStateChanged.Playing:
                        break;
                    case AudioOutputStateChanged.Paused:
                        break;
                    case AudioOutputStateChanged.Finished:
                        break;
                }
            };
            State = InitState(new PlayerState());
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
                Session.FinishedLoading -= SessionOnFinishedLoading;

            Session = new PlayerSession(WsState,
                sessionId);
            Session.FinishedLoading += SessionOnFinishedLoading;
            //_events.SendEvent(new NewSessionIdEvent(sessionId, _stateWrapper).BuildEvent());

            await LoadTrack(play, transitionInfo);
        }

        private void SessionOnFinishedLoading(object sender, ChunkedStream e)
        {
            EnrichWithMetadata(e);
            State.IsBuffering = false;
            _ = WsState.UpdateState(PutStateReason.PlayerStateChanged, WsState.AudioOutput
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
                WsState.AudioOutput.Resume(State.Position);
            else
                WsState.AudioOutput.Pause();
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
