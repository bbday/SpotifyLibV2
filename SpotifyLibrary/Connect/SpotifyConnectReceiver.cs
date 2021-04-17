using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Connectstate;
using Extensions;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Nito.AsyncEx;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyConnectReceiver : ISpotifyConnectReceiver
    {
        private readonly AsyncLock clusterLock = new AsyncLock();
        internal readonly ISpotifyLibrary Library;
        private ManualResetEvent _waitForConid = null!;
        private SpotifyMessageState _messageState;
        private SpotifyRequestState _requestState;
        internal SpotifyConnectReceiver(ISpotifyLibrary library)
        {
            Library = library;
        }

        internal async Task<PlayingItem> Connect(IWebsocketClient ws)
        {
            var dealerClient = new DealerClient((Library.CurrentUser as PrivateUser)!
                .Uri.Split(':').Last(),
                Library.Tokens, ws);
            dealerClient.Attach();
            _messageState = new SpotifyMessageState(dealerClient, this);
            _requestState = new SpotifyRequestState(Library, dealerClient, this);
            var connected = await dealerClient.Connect();
            WaitForConnectionId.WaitOne();
            return LastReceivedCluster;
        }


        public IRemoteDevice ActiveDevice { get; } = null!;
        public event EventHandler<RepeatState>? RepeatStateChanged;
        public event EventHandler<double>? PositionChanged;
        public event EventHandler<bool>? IsShuffleCHanged;
        public event EventHandler<bool>? IsPausedChanged;
        public event EventHandler<(PlayingItem Item, IRemoteDevice Device)>? NewItem;
        internal ManualResetEvent WaitForConnectionId
        {
            get { return _waitForConid ??= new ManualResetEvent(false); }
        }

        public PlayingItem? LastReceivedCluster { get; private set; } = null!;

        internal async void OnNewPlaybackWrapper(object sender, PlayerState state)
        {
            var newWrapper = await LocalStateToWrapper(state);
            LastReceivedCluster = newWrapper;

            NewItem?.Invoke(sender, (LastReceivedCluster, ActiveDevice));

            WaitForConnectionId.Set();
        }
        internal void OnRepeatStateChanged(object sender, RepeatState state)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.RepeatState = state;
            }
            RepeatStateChanged?.Invoke(sender, state);
        }
        internal void OnPositionChanged(object sender, double pos)
        {
            PositionChanged?.Invoke(sender, pos);
        }

        internal void OnShuffleStatecHanged(object sender, bool isShuffling)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.IsShuffle = isShuffling;
            }
            IsShuffleCHanged?.Invoke(sender, isShuffling);
        }

        internal void OnPlaybackStateChanged(object sender, bool paused)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.IsPaused = paused;
            }
            IsPausedChanged?.Invoke(sender, paused);
        }
        private async Task<PlayingItem> LocalStateToWrapper(
       PlayerState currentState)
        {
            using (await clusterLock.LockAsync())
            {
                RepeatState repeatState;

                var repeatingTrack = currentState.Options.RepeatingTrack;
                var repeatingContext = currentState.Options.RepeatingContext;
                if (repeatingContext && !repeatingTrack)
                {
                    repeatState = RepeatState.Context;
                }
                else
                {
                    if (repeatingTrack)
                    {
                        repeatState = RepeatState.Track;
                    }
                    else
                    {
                        repeatState = RepeatState.Off;
                    }
                }

                var contextId = currentState.ContextUri?.UriToIdConverter();

                var itemId = currentState.Track.Uri?.UriToIdConverter();

                ITrackItem? item = null;
                var descriptions = new List<Descriptions>();
                var durationMs = 0;
                IAudioId? groupId = null;
                switch (itemId.AudioType)
                {
                    case AudioItemType.Track:
                        var tracksClient = await Library.TracksClient;
                        var fullTrack = await tracksClient.GetTrack(itemId.Id);
                        descriptions.AddRange(fullTrack.Artists.Select(z =>
                            new Descriptions(z.Name, new ArtistId((z as SimpleArtist).Uri))));
                        durationMs = fullTrack.DurationMs;
                        item = fullTrack;
                        groupId = fullTrack.Group.Id;
                        break;
                    case AudioItemType.Episode:
                        var episodesClient = await Library.EpisodesClient;
                        var fullEpisode = await episodesClient.GetEpisode(itemId.Id);
                        descriptions.AddRange(new Descriptions[]
                        {
                            new Descriptions(fullEpisode.Show.Name, fullEpisode.Show.Id)
                        });
                        durationMs = fullEpisode.DurationMs;
                        item = fullEpisode;
                        groupId = fullEpisode.Show.Id;
                        break;
                    default:
                        throw new NotImplementedException("?");
                }

                var clustered = new PlayingItem(item, groupId, repeatState,
                    currentState.Options.ShufflingContext,
                    currentState.IsPaused,
                    null,
                    contextId,
                    currentState.Timestamp,
                    currentState.PositionAsOfTimestamp, descriptions, TimeSpan.FromMilliseconds(durationMs));
                LastReceivedCluster = clustered;
                return clustered;
            }
        }

        public Task UpdateConnectionId(string header) => _requestState.UpdateConnectionId(header);

        public void OnNewCluster(Cluster tryGet)
        {
            _messageState.PreviousCluster = tryGet;
        }
    }
}
