using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Interfaces;

namespace SpotifyLibV2.Connect
{
    public class SpotifyConnectClient :
        ISpotifyConnectClient
    {
        private readonly ISpotifyPlayer _player;
        private readonly IDealerClient _dealerClient;
        private readonly ISpotifyConnectState _connectState;

        public SpotifyConnectClient(
            IDealerClient dealerClient, 
            ISpotifyPlayer player, 
            ISpotifyConnectReceiver receiver, 
            IEventsService events,
            SpotifyConfiguration config)
        {
            _dealerClient = dealerClient;
            _player = player;

            _connectState = new SpotifyConnectState(dealerClient, receiver,
                config,
                0,
                100);
        }
        public Task<bool> Connect() => _dealerClient.Connect();
    }
}
