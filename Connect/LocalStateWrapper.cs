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


        private readonly PagesLoader _pages;
        private TracksKeeper.TracksKeeper _tracksKeeper;
        private readonly APWelcome _apWelcome;
        private readonly ISpotifyPlayer _player;
        private readonly SpotifyConfiguration _playerConfiguration;
        private ConcurrentDictionary<string, string> _attributes;
        private readonly PlayerState _state;

        internal LocalStateWrapper(ISpotifyPlayer player,
            SpotifyConfiguration playerConfiguration, 
            ConcurrentDictionary<string, string> attributes, APWelcome apWelcome)
        {
            _player = player;
            _playerConfiguration = playerConfiguration;
            _attributes = attributes;
            _apWelcome = apWelcome;
            _state = InitState(new PlayerState());
        }

        public AbsSpotifyContext Context { get; }

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

        private void SetState(bool playing, bool paused, bool buffering)
        {
            if (paused && !playing) throw new IllegalStateException();
            else if (buffering && !playing) throw new IllegalStateException();

            var wasPaused = IsPaused;
            _state.IsPlaying = playing;
            _state.IsPaused = paused;
            _state.IsBuffering = buffering;

        //    if (wasPaused && !paused) // Assume the position was set immediately before pausing
               // SetPosition(_state.PositionAsOfTimestamp);
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
                        //TODO: return _tracksKeeper size
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
            PlayableId.FromUri(_state.Track.Uri);

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
