using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using Google.Protobuf.Collections;
using JetBrains.Annotations;
using Org.BouncyCastle.Utilities.Encoders;
using Spotify;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Contexts;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Connect.Restrictions;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;
using SpotifyLibV2.Models;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;

namespace SpotifyLibV2.Connect
{
    internal class LocalStateWrapper : IDeviceStateListener, IMessageListener, IDisposable
    {
        static readonly char[] padding = { '=' };

        static LocalStateWrapper()
        {
            try
            {
                ProtoUtils.OverrideDefaultValue(ContextIndex.Descriptor.FindFieldByName("track"), -1);
                ProtoUtils.OverrideDefaultValue(
                    PlayerState.Descriptor
                        .FindFieldByName("position_as_of_timestamp"), -1);
                ProtoUtils.OverrideDefaultValue(
                    ContextPlayerOptions.Descriptor.FindFieldByName("shuffling_context"), "");
                ProtoUtils.OverrideDefaultValue(ContextPlayerOptions.Descriptor.FindFieldByName("repeating_track"),
                    "");
                ProtoUtils.OverrideDefaultValue(
                    ContextPlayerOptions.Descriptor.FindFieldByName("repeating_context"), "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed changing default value!", ex);
            }
        }


        public PagesLoader Pages
        {
            get;
            private set;
        }
        private TracksKeeper.TracksKeeper _tracksKeeper;
        private readonly APWelcome _apWelcome;
        private readonly ISpotifyPlayer _player;
        private readonly SpotifyConfiguration _playerConfiguration;
        private ConcurrentDictionary<string, string> _attributes;
        private readonly PlayerState _state;
        private readonly IMercuryClient _mercuryClient;
        private readonly ISpotifyConnectState _connectState;
        internal LocalStateWrapper(ISpotifyPlayer player,
            SpotifyConfiguration playerConfiguration, 
            ConcurrentDictionary<string, string> attributes, APWelcome apWelcome, IMercuryClient mercuryClient, 
            ISpotifyConnectState connectState)
        {
            _player = player;
            _playerConfiguration = playerConfiguration;
            _attributes = attributes;
            _apWelcome = apWelcome;
            _mercuryClient = mercuryClient;
            _connectState = connectState;
            _state = InitState(new PlayerState());
        }

        public string SessionId => _state.SessionId;
        public AbsSpotifyContext Context { get; private set; }

        public void SetPlaybackId(string playbackId) => _state.PlaybackId = playbackId;
        private static PlayerState InitState([NotNull] PlayerState builder)
        {
            builder.PlaybackSpeed = 1;
            builder.SessionId = string.Empty;
            builder.PlaybackId = string.Empty;
            builder.Suppressions = new Suppressions();
            builder.ContextRestrictions = new Connectstate.Restrictions();
            builder.Options = new ContextPlayerOptions
            {
                RepeatingContext = false,
                RepeatingTrack = false,
                ShufflingContext = false
            };
            builder.PositionAsOfTimestamp = 0;
            builder.Position = 0;
            builder.IsPlaying = false;
            return builder;
        }


        public async Task<string> Transfer(TransferState command)
        {
            var ps = command.CurrentSession;

            _state.PlayOrigin = ProtoUtils.ConvertPlayOrigin(ps.PlayOrigin);
            _state.Options = ProtoUtils.ConvertPlayerOptions(command.Options);
            var sessionId = SetContext(ps.Context);

            var pb = command.Playback;
            try
            {
                _tracksKeeper.InitializeFrom(list =>
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var track = list[i];
                        if (track.HasUid && ps.CurrentUid.Equals(track.Uid) ||
                            ProtoUtils.TrackEquals(track, pb.CurrentTrack))
                        {
                            return i;
                        }
                    }
                    return -1;
                }, pb.CurrentTrack, command.Queue);
            }
            catch(IllegalStateException ex)
            {
                Debug.Write(ex.ToString());
                _tracksKeeper.InitializeStart();
            }

            _state.PositionAsOfTimestamp = pb.PositionAsOfTimestamp;
            if (pb.IsPaused) _state.Timestamp = TimeProvider.CurrentTimeMillis();
            else _state.Timestamp = pb.Timestamp;

            LoadTransforming();
            return sessionId;
        }

        public void LoadTransforming()
        {
            if (_tracksKeeper == null) throw new IllegalStateException();

            var url = _state.ContextMetadata.GetMetadataOrDefault("transforming.url", null);
            if (url == null) return;

            var shuffle = false;
            if (_state.ContextMetadata.ContainsKey("transforming.shuffle"))
                shuffle = bool.Parse(_state.ContextMetadata["transforming.shuffle"]);

            var willRequest = !_state.Track.Metadata.ContainsKey("audio.fwdbtn.fade_overlap");
            Debug.WriteLine($"Context has transforming: {url}, shuffle: {shuffle}, willRequest: {willRequest}");

            if (!willRequest) return;

            //Todo ?
        }
        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            //TODO: Prolly gonna have to rework playlist services/handlers/listeners for now we can ignore.
            return Task.CompletedTask;
            /*if (uri.StartsWith("hm://playlist/"))
            {
                //Decode playlist operators
                Debugger.Break();
            }
            else if (Context != null && AbsSpotifyContext.IsCollection(_apWelcome, uri))
            {

            }*/
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Ready()
        {
            throw new NotImplementedException();
        }

        public void Command(Endpoint endpoint, CommandBody data)
        {
            //Ignore
        }

        public void VolumeChanged()
        {
            throw new NotImplementedException();
        }

        public void NotActive()
        {
            throw new NotImplementedException();
        }

        public static string GeneratePlaybackId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            bytes[0] = 1;
            return bytes.BytesToHex().ToLowerInvariant();
        }
        private static string GenerateSessionId()
        {
            byte[] bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            return Convert.ToBase64String(bytes)
                .TrimEnd(padding).Replace('+', '-').Replace('/', '_');
        }
        private bool ShouldPlay([NotNull] ContextTrack track)
        {
            if (!PlayableId.IsSupported(track.Uri) || !PlayableId.ShouldPlay(track))
                return false;

            var filterExplicit = string.Equals(_attributes["filter-explicit-content"], "1");
            if (!filterExplicit) return true;

            return !bool.Parse(track.Metadata.GetMetadataOrDefault("is_explicit", "false"));
        }

        public bool IsActive() => _player.IsActive;
        private string SetContext(Context ctx)
        {
            var uri = ctx.Uri;
            Context = AbsSpotifyContext.From(uri);
            _state.ContextUri = uri;

            if (!Context.IsFinite())
            {
                SetRepeatingContext(false);
                SetShufflingContext(false);
            }

            if (ctx.HasUrl) _state.ContextUrl = ctx.Url;
            else _state.ContextUrl = string.Empty;

            _state.ContextMetadata.Clear();
            ProtoUtils.CopyOverMetadata(ctx, _state);
            

            Pages = PagesLoader.From(_mercuryClient, uri);
            _tracksKeeper = new TracksKeeper.TracksKeeper(this, _state, Context);

            _player.IsActive = true;

            return RenewSessionId();
        }
        private string SetContext(string uri)
        {
            Context = AbsSpotifyContext.From(uri);
            _state.ContextUri = uri;

            if (!Context.IsFinite())
            {
                SetRepeatingContext(false);
                SetShufflingContext(false);
            }

            _state.ContextUrl = string.Empty;
            _state.Restrictions = new Connectstate.Restrictions();
            _state.ContextRestrictions = new Connectstate.Restrictions();
            _state.ContextMetadata.Clear();

            Pages = PagesLoader.From(_mercuryClient, uri);
            _tracksKeeper = new TracksKeeper.TracksKeeper(this, _state, Context);

            _player.IsActive = true;

            return RenewSessionId();
        }

        public double Position => _state.Position;
        public void SetPosition(long pos)
        {
            _state.Timestamp = TimeProvider.CurrentTimeMillis();
            _state.PositionAsOfTimestamp = pos;
            _state.Position = 0L;
        }

        public void UpdateRestrictions()
        {
            if (Context == null) return;

            if(_tracksKeeper.IsPlayingFirst() && !IsRepeatingContext)
                Context.Restrictions.Disallow(RestrictionsManager.Action.SKIP_PREV, RestrictionsManager.REASON_NO_PREV_TRACK);
            else
                Context.Restrictions.Allow(RestrictionsManager.Action.SKIP_PREV);

            if (_tracksKeeper.IsPlayingLast() && !IsRepeatingContext)
                Context.Restrictions.Disallow(RestrictionsManager.Action.SKIP_NEXT, RestrictionsManager.REASON_NO_NEXT_TRACK);
            else
                Context.Restrictions.Allow(RestrictionsManager.Action.SKIP_NEXT);

            _state.Restrictions = Context.Restrictions.ToProto();
            _state.ContextRestrictions = Context.Restrictions.ToProto();
        }
        public Task Updated()
        {
            UpdateRestrictions(); 
            return _connectState.UpdateState(PutStateReason.PlayerStateChanged, _player.Time, _state);
        }
        public void SetState(bool playing, bool paused, bool buffering)
        {
            if (paused && !playing) throw new IllegalStateException();
            else if (buffering && !playing) throw new IllegalStateException();

            var wasPaused = IsPaused;
            _state.IsPlaying = playing;
            _state.IsPaused = paused;
            _state.IsBuffering = buffering;

            if (wasPaused && !paused) // Assume the position was set immediately before pausing
                SetPosition(_state.PositionAsOfTimestamp);
        }

        public string ContextUrl => _state.ContextUrl;
        public uint ContextSize
        {
            get
            {
                var trackCount = _state.ContextMetadata.GetMetadataOrDefault("track_count", null);
                if (!int.TryParse(trackCount, out var trackCountInt))
                {
                    if (_tracksKeeper != null)
                    {
                        return (uint)_tracksKeeper.Tracks.Count;
                    }
                    else
                    {
                        trackCountInt = 0;
                    }
                }

                return (uint)trackCountInt;
            }
        }

        public IPlayableId GetPlayableItem => _tracksKeeper == null 
            ? null : 
            PlayableId.From(_state.Track);

        public bool IsPaused => _state.IsPlaying && _state.IsPaused;
        public bool IsShufflingContext => _state.Options.ShufflingContext;
        public bool IsRepeatingContext => _state.Options.RepeatingContext;
        public bool IsRepeatingTrack => _state.Options.RepeatingTrack;

        public void SetBuffering(bool buffering) => SetState(true, _state.IsPaused, buffering);

        public void SetShufflingContext(bool value)
        {
            if (Context == null 
                || _tracksKeeper == null) return;

            var old = IsShufflingContext;
            _state.Options.ShufflingContext = value && Context.Restrictions.Can(RestrictionsManager.Action.SHUFFLE);

            if (old != IsShufflingContext)
                _tracksKeeper.ToggleShuffle(IsShufflingContext);

        }

        public void SetRepeatingContext(bool value)
        {
            if(Context == null) return;
            
            _state.Options.RepeatingContext = true && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_CONTEXT);
        }

        public void SetRepeatingTrack(bool value)
        {
            if (Context == null) return;
            _state.Options.RepeatingTrack = true && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_TRACK);
        }

        private string RenewSessionId()
        {
            var sessionId = GenerateSessionId();
            _state.SessionId = sessionId;
            return sessionId;
        }
    }

    public static class MetadataItemsExtensions
    {
        public static string GetMetadataOrDefault(this MapField<string, string> mapField, string key, string defaultValue)
        {
            var exists = mapField.ContainsKey(key);
            return exists ? mapField[key] : defaultValue;
        }
    }
}
