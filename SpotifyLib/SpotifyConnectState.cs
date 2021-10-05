using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using Spotify.Player.Proto;
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

            Session = new PlayerSession(WsState.AudioOutput,
                sessionId);
            //_events.SendEvent(new NewSessionIdEvent(sessionId, _stateWrapper).BuildEvent());

            await LoadTrack(play, transitionInfo);
        }

        private async Task LoadTrack(bool willPlay, TransitionInfo transitionInfo)
        {
            Debug.WriteLine($"Loading track id: {GetCurrentPlayable.Uri}");
            var playbackId = await Session.Load(GetCurrentPlayable,
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
                if (id.Uri == null) throw new IllegalStateException();
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
    }
}
