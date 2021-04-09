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
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
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
using SpotifyLibrary.Models.Request.Playback;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.SpotifyItems;
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
        public event EventHandler<double> PositionChanged;
        public event EventHandler<RepeatState> RepeatStateChanged;
        public event EventHandler<List<IQueueUpdateItem>> QueueUpdated;
        public DealerClient DealerClient
        {
            get => _dealerClient;
            set
            {
                _dealerClient = value;
                _request = new SpotifyRequestState(value, this);
                _messages = new SpotifyMessageState(value, _request, this);
            }
        }


        public ISpotifyPlayer Player { get; }

        public PlayingItem LastReceivedCluster { get; private set; }

        public ManualResetEvent WaitForConnectionId
        {
            get { return _waitForConid ??= new ManualResetEvent(false); }
        }

        public string CurrentDevice => _request.IsActive 
            ? _client.Config.DeviceId : _messages.CurrentDeviceId;

        public async Task<AcknowledgedResponse> InvokeCommandOnRemoteDevice(RemoteCommand playbackState,
            string deviceId = null)
        {
            var connetState = await Client.ConnectState;
            switch (playbackState)
            {
                case RemoteCommand.Pause:
                    return await connetState.Command(Client.Config.DeviceId, deviceId ?? CurrentDevice, new
                    {
                        command = new
                        {
                            endpoint = "pause"
                        }
                    });
                case RemoteCommand.Play:
                    return await connetState.Command(Client.Config.DeviceId, deviceId ?? CurrentDevice, new
                    {
                        command = new
                        {
                            endpoint = "resume"
                        }
                    });
                    break;
                case RemoteCommand.Skip:
                    break;
                case RemoteCommand.Previous:
                    break;
                case RemoteCommand.ShuffleToggle:
                    break;
                case RemoteCommand.RepeatContext:
                    break;
                case RemoteCommand.RepeatTrack:
                    break;
                case RemoteCommand.RepeatOff:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playbackState), playbackState, null);
            }
            throw new ArgumentOutOfRangeException(nameof(playbackState), playbackState, null);
        }

        public async Task<AcknowledgedResponse> PlayItem(string connectClientCurrentDevice,
            IPlayRequest request)
        {
            var connetState = await Client.ConnectState;

            return await connetState.Command(Client.Config.DeviceId, connectClientCurrentDevice ?? CurrentDevice, request.GetModel()); ;
        }

        public async Task<AcknowledgedResponse> Seek(double delta,
            string deviceId = null)
        {
            var connetState = await Client.ConnectState;

            return await connetState.Command(Client.Config.DeviceId, 
                deviceId ?? CurrentDevice, new
            {
                command = new
                {
                    endpoint = "seek_to",
                    value = delta
                }
            });
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
        internal void OnNewCluster(Cluster state)
        {
            _messages.PreviousCluster = state;
        }
        public async void OnNewPlaybackWrapper(object sender, PlayerState state)
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
                IAudioId? groupId = null;
                switch (itemId.AudioType)
                {
                    case AudioType.Track:
                        var tracksClient = await _client.TracksClient;
                        var fullTrack = await tracksClient.GetTrack(itemId.Id);
                        descriptions.AddRange(fullTrack.Artists.Select(z =>
                            new Descriptions(z.Name, new ArtistId((z as SimpleArtist).Uri))));
                        durationMs = fullTrack.DurationMs;
                        item = fullTrack;
                        groupId = fullTrack.Group.Id;
                        break;
                    case AudioType.Episode:
                        var episodesClient = await _client.EpisodesClient;
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

                var clustered = new PlayingItem(item, groupId,  repeatState,
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

        public void OnNewItemsInQueue(object sender, List<IQueueUpdateItem> items)
        {
            QueueUpdated?.Invoke(sender, items);
        }
    }
}