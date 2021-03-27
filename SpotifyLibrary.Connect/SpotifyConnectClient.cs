using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SpotifyLibrary.Connect.Player;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Models;
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
        }

        private ITokensProvider Tokens => _client.Tokens;
        private SpotifyConfiguration Config => _client.Config;

        public SpotifyClient Client
        {
            get => _client;
            set => _client = value;
        }

        public event EventHandler<PlayingItem> NewPlaybackWrapper;

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

        public PlayingItem LastReceivedCluster
        {
            get; private set;
        }

        public ManualResetEvent WaitForConnectionId
        {
            get
            {
                return _waitForConid ??= new ManualResetEvent(false);
            }
        }

        internal async void OnNewPlaybackWrapper(Cluster update)
        {
            if (update.PlayerState != null)
            {
                var newWrapper = await ClusterToWrapper(update);
                LastReceivedCluster = newWrapper;
                NewPlaybackWrapper?.Invoke(this, LastReceivedCluster);
            }
            WaitForConnectionId.Set();
        }

        public Task UpdateConnectionId(string header) => _request.UpdateConnectionId(header);
        private AsyncLock clusterLock = new AsyncLock();
        private async Task<PlayingItem> ClusterToWrapper(Cluster cluster)
        {
            using (await clusterLock.LockAsync())
            {
                RepeatState repeatState;

                var repeatingTrack = cluster.PlayerState.Options.RepeatingTrack;
                var repeatingContext = cluster.PlayerState.Options.RepeatingContext;
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

                var contextId = cluster.PlayerState.ContextUri?.UriToIdConverter();

                var itemId = cluster.PlayerState.Track.Uri?.UriToIdConverter();

                IAudioItem? item = null;
                switch (itemId.AudioType)
                {
                    case AudioType.Track:
                        var tracksClient = await _client.TracksClient;
                        item = await tracksClient.GetTrack(itemId.Id);
                        break;
                    case AudioType.Episode:
                        break;
                    default:
                        throw new NotImplementedException("?");
                }

                var clustered = new PlayingItem(item, repeatState,
                    cluster.PlayerState.Options.ShufflingContext,
                    cluster.PlayerState.IsPaused,
                    null,
                    contextId,
                    cluster.PlayerState.Timestamp,
                    cluster.PlayerState.PositionAsOfTimestamp);
                LastReceivedCluster = clustered;
                return clustered;
            }
        }
    }
}
