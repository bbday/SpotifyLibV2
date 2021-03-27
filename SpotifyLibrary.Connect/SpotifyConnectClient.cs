using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using JetBrains.Annotations;
using Nito.AsyncEx;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Connect.Enums;
using SpotifyLibrary.Connect.Player;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Interfaces;
using SpotifyLibrary.Player;
using SpotifyLibrary.Services.Mercury;
using SpotifyLibrary.Services.Mercury.Interfaces;

namespace SpotifyLibrary.Connect
{
    public class SpotifyConnectClient : ISpotifyConnectClient
    {
        private SpotifyClient _client;
        private SpotifyMessageState _messages;
        private SpotifyRequestState _request;
        private DealerClient _dealerClient;
        private ManualResetEvent _waitForConid;

        public SpotifyConnectClient()
        {

        }

        public SpotifyConnectClient(ISpotifyPlayer player)
        {
            Player = player;
            Player.Session = Client;
        }

        private ITokensProvider Tokens => _client.Tokens;
        private SpotifyConfiguration Config => _client.Config;

        public SpotifyClient Client
        {
            get => _client;
            set => _client = value;
        }

        public event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        public event EventHandler<PlayingItem> NewPlaybackWrapper;
        public event EventHandler<bool> ShuffleStateChanged;
        public event EventHandler<RepeatState> RepeatStateChanged;

        public DealerClient DealerClient
        {
            get => _dealerClient;
            set
            {
                _dealerClient = value;
                _request = new SpotifyRequestState(value, this);
                _messages = new SpotifyMessageState(value, this);
            }
        }


        public ISpotifyPlayer Player { get; }

        public PlayingItem LastReceivedCluster { get; private set; }

        public ManualResetEvent WaitForConnectionId
        {
            get { return _waitForConid ??= new ManualResetEvent(false); }
        }

        internal void OnRepeatStateChanged(object sender, RepeatState state)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.RepeatState = state;
            }
            RepeatStateChanged?.Invoke(sender, state);
        }

        internal void OnShuffleStatecHanged(object sender, bool isShuffling)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.IsShuffle = isShuffling;
            }
            ShuffleStateChanged?.Invoke(sender, isShuffling);
        }

        internal void OnPlaybackStateChanged(object sender, MediaPlaybackState newState)
        {
            if (LastReceivedCluster != null)
            {
                LastReceivedCluster.IsPaused = newState == MediaPlaybackState.TrackPaused;
            }
            PlaybackStateChanged?.Invoke(sender, newState);
        }

        internal async void OnNewPlaybackWrapper(object sender, PlayerState state)
        {
            var newWrapper = await LocalStateToWrapper(state);
            LastReceivedCluster = newWrapper;

            NewPlaybackWrapper?.Invoke(sender, LastReceivedCluster);

            WaitForConnectionId.Set();
        }

        public Task UpdateConnectionId(string header) => _request.UpdateConnectionId(header);
        private AsyncLock clusterLock = new AsyncLock();

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

                IAudioItem? item = null;
                var descriptions = new List<Descriptions>();
                var durationMs = 0;
                switch (itemId.AudioType)
                {
                    case AudioType.Track:
                        var tracksClient = await _client.TracksClient;
                        var fullTrack = await tracksClient.GetTrack(itemId.Id);
                        descriptions.AddRange(fullTrack.Artists.Select(z =>
                            new Descriptions(z.Name, new ArtistId(z.Uri))));
                        durationMs = fullTrack.DurationMs;
                        item = fullTrack;
                        break;
                    case AudioType.Episode:
                        break;
                    default:
                        throw new NotImplementedException("?");
                }

                var clustered = new PlayingItem(item, repeatState,
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
    }
}