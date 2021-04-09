using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MusicLibrary.Enum;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Connect.Enums;
using SpotifyLibrary.Connect.Player;
using SpotifyLibrary.Connect.Qualities;
using SpotifyLibrary.Connect.Transitions;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Player;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Connect.PlayerSession
{
    public class PlayerQueueEntry : IHaltListener
    {
        private ISpotifyPlayer _player;
        private readonly IPlayerQueueListener _listener;
        private readonly ICdnManager _cdnManager;
        public PlayerQueueEntry(
            ISpotifyPlayer player,
            ISpotifyId playable,
            IPlayerQueueListener listener, ICdnManager cdnManager)
        {
            this._player = player;
            this.Id = playable;
            _listener = listener;
            _cdnManager = cdnManager;
            PlaybackId = LocalStateWrapper.GeneratePlaybackId();

            player.StateChanged -= StateChanged;
            player.StateChanged += StateChanged;
        }

        private void StateChanged(MediaPlaybackState state, TrackOrEpisode metadata)
        {
            switch (state)
            {
                case MediaPlaybackState.NewTrack:
                    break;
                case MediaPlaybackState.TrackPlayed: 
                    _listener.PlaybackEnded(this);
                    break;
            }
        }

        public async Task Start()
        {
            _listener.StartedLoading(this);

            try
            {
                var currentStream = await _cdnManager.LoadTrack(Id as TrackId,
                    new VorbisOnlyAudioQuality(AudioQualityHelper.AudioQuality.HIGH),
                    false,
                    this);
                Metadata = new TrackOrEpisode(currentStream.Track, currentStream.Episode);
                Debug.WriteLine($"Loaded item... {Id.Id}");



                await _player.StreamReady(currentStream);
            }
            catch (Exception x)
            {
                _listener.LoadingError(this, x, false);
                Debug.WriteLine("{0} terminated at loading.", x);
                return;
            }

            _listener.FinishedLoading(this, Metadata);
        }

        public bool Equals(PlayerQueueEntry other)
        {
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerQueueEntry other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }


        public ISpotifyId Id { get; }
        public string PlaybackId { get; }
        public TrackOrEpisode Metadata { get; private set; }
        public TransitionReason EndReason { get; private set; }


        public void StreamReadHalted(int chunk, long time)
        {
            throw new NotImplementedException();
        }

        public void StreamReadResumed(int chunk, long time)
        {
            throw new NotImplementedException();
        }
    }
}
