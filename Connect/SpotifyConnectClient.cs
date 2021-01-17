using Nito.AsyncEx;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Models.Public;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SpotifyLibV2.Models.Request;

namespace SpotifyLibV2.Connect
{
    public class SpotifyConnectClient :
        ISpotifyConnectClient
    {
        private readonly ISpotifyPlayer _player;
        private readonly IDealerClient _dealerClient;
        private readonly ISpotifyConnectState _connectState;
        private readonly AsyncLazy<IPlayerClient> _playerApi;
        private readonly SpotifyConfiguration _config;
        private readonly AsyncLazy<IConnectState> _connectApi;
        public SpotifyConnectClient(
            IDealerClient dealerClient, 
            ISpotifyPlayer player, 
            ISpotifyConnectReceiver receiver, 
            IEventsService events,
            ITokensProvider tokens,
            AsyncLazy<IConnectState> connectApi,
            AsyncLazy<IPlayerClient> playerApi,
            SpotifyConfiguration config)
        {
            _dealerClient = dealerClient;
            _player = player;
            _playerApi = playerApi;
            _connectApi = connectApi;
            _config = config;
            _connectState = new SpotifyConnectState(dealerClient, receiver,
                tokens,
                config,
                0,
                100);
        }
        public Task<bool> Connect() => _dealerClient.Connect();

        public async Task<PlayingChangedRequest?> FetchCurrentlyPlaying()
        {
            var playerApi = await _playerApi;
            var playback = await playerApi.GetCurrentPlayback();
            if (playback != null)
            {
                return new PlayingChangedRequest(
                    (RepeatState)Enum.Parse(typeof(RepeatState), playback.RepeatState, true),
                    playback.ShuffleState,
                    playback.Item.Uri,
                    playback.Context?.Uri,
                    !playback.IsPlaying,
                    playback.IsPlaying, playback.Timestamp);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> RequestPlay(RemoteRequest request)
        {
            if (_connectState.ActiveDeviceId == null)
            {
                var t = await (await _playerApi).GetCurrentPlayback();
                _connectState.ActiveDeviceId = t.Device?.Id;
            }
            var resp =
                await (await _connectApi).TransferState(_config.DeviceId,
                    _connectState.ActiveDeviceId, request);
            if (!resp.IsSuccessStatusCode)
            {
                Debugger.Break();
                return false;
            }

            return true;
        }
    }
}
