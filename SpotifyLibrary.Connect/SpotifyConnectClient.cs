using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Connect.Player;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models;
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



        public event EventHandler<PlaybackItemWrapper> NewPlaybackWrapper;

        public ISpotifyPlayer Player { get; }

        internal void OnNewPlaybackWrapper(Cluster update)
        {
            NewPlaybackWrapper?.Invoke(this, new PlaybackItemWrapper(update.PlayerState.Track.Uri));
        }

        public void UpdateConnectionId(string header) => _request.UpdateConnectionId(header);
    }
}
