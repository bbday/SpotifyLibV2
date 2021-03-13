using Nito.AsyncEx;
using SpotifyLibV2.Api;
using SpotifyLibV2.Config;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Models.Public;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Spotify;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Request.PlaybackRequests;

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
        private readonly APWelcome _apWelcome;

        public SpotifyConnectClient(
            IDealerClient dealerClient, 
            ISpotifyPlayer player, 
            ISpotifyConnectReceiver receiver, 
            IEventsService events,
            ConcurrentDictionary<string, string> attributes,
            ITokensProvider tokens,
            AsyncLazy<IConnectState> connectApi,
            AsyncLazy<IPlayerClient> playerApi,
            SpotifyConfiguration config, APWelcome apWelcome)
        {
            _dealerClient = dealerClient;
            _player = player;
            _playerApi = playerApi;
            _connectApi = connectApi;
            _config = config;
            _apWelcome = apWelcome;
            _connectState = new SpotifyConnectState(dealerClient,
                player,
                receiver,
                tokens,
                config, attributes,
                0,
                100, apWelcome, events);
        }

        public string ActiveDeviceId => _connectState?.ActiveDeviceId;
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
                    playback.Item?.Uri,
                    playback.Context?.Uri,
                    !playback.IsPlaying,
                    playback.IsPlaying, 
                    playback.ProgressMs,
                    playback.Device);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> RequestPlay(IPlaybackRequest request)
        {
            if (_connectState.ActiveDeviceId == null)
            {
                var t = await (await _playerApi).GetCurrentPlayback();
                _connectState.ActiveDeviceId = t.Device?.Id;
            }
            var resp =
                await (await _connectApi).TransferState(_config.DeviceId,
                    _connectState.ActiveDeviceId, 
                    "player",
                    "command", request.GetModel());
            if (!resp.IsSuccessStatusCode)
            {
                Debugger.Break();
                return false;
            }

            return true;
        }

        public async Task<bool> TransferDevice(string deviceId)
        {
            var transferRequest = new TransferRequst();
            if (_connectState.ActiveDeviceId == null)
            {
                var t = await(await _playerApi).GetCurrentPlayback();
                _connectState.ActiveDeviceId = t.Device?.Id;
            }
            var resp =
                await(await _connectApi).TransferState(_config.DeviceId,
                    deviceId,
                    "connect",
                    "transfer", transferRequest.GetModel());
            if (!resp.IsSuccessStatusCode)
            {
                Debugger.Break();
                return false;
            }

            return true;
        }

        internal void AttachListener(PlaylistId uri, IPlaylistListener listener) =>
            _dealerClient.AddPlaylistListener(listener, uri);
    }
}
