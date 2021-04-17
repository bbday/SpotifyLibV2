using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using MediaLibrary.Enums;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyMessageState : IMessageListener
    {
        private readonly SpotifyConnectReceiver _spotifyConnectClient;
        private readonly DealerClient _dealerClient;
        private readonly SpotifyRequestState _requestState;
        private RepeatState? _previousRepeatState;

        internal SpotifyMessageState(DealerClient dealerClient,
            SpotifyConnectReceiver onNewCluster)
        {
            _dealerClient = dealerClient;
            //_requestState = requestState;
            _spotifyConnectClient = onNewCluster;
            dealerClient.AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
        }
        internal Cluster PreviousCluster
        {
            get;
            set;
        }
        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            if (uri.StartsWith("hm://pusher/v1/connections/"))
            {
                return _spotifyConnectClient.UpdateConnectionId(headers["Spotify-Connection-Id"]);
            }
            else if (uri.StartsWith("hm://connect-state/v1/cluster"))
            {
                var update = ClusterUpdate.Parser.ParseFrom(payload);
                if (update.Cluster?.PlayerState != null)
                {
                    if (update.Cluster.PlayerState?.Track?.Uri != null)
                    {
                        if (PreviousCluster?.PlayerState?.Track?.Uri
                            != update.Cluster.PlayerState.Track.Uri)
                        {
                            _spotifyConnectClient.OnNewPlaybackWrapper(this, update.Cluster.PlayerState);
                        }
                    }
                    if (PreviousCluster?.PlayerState != null && update.Cluster?.PlayerState != null)
                    {
                        if (PreviousCluster.PlayerState.IsPaused != update.Cluster.PlayerState.IsPaused)
                        {
                            _spotifyConnectClient.OnPlaybackStateChanged(this,
                                update.Cluster.PlayerState.IsPaused);
                        }
                    }
                    if (PreviousCluster?.PlayerState?.Options == null)
                    {
                        _spotifyConnectClient.OnShuffleStatecHanged(this, 
                            update.Cluster!.PlayerState!.Options.ShufflingContext);
                        _previousRepeatState = ParseRepeatState(update.Cluster);
                        _spotifyConnectClient.OnRepeatStateChanged(this, _previousRepeatState.Value);
                    }
                    else
                    {
                        var newRepeatState = ParseRepeatState(update.Cluster!);
                        if (_previousRepeatState != newRepeatState)
                        {
                            _spotifyConnectClient.OnRepeatStateChanged(this, newRepeatState);
                            _previousRepeatState = newRepeatState;
                        }

                        if (PreviousCluster.PlayerState.Options.ShufflingContext !=
                            update.Cluster!.PlayerState!.Options.ShufflingContext)
                        {
                            _spotifyConnectClient.OnShuffleStatecHanged(this,
                                update.Cluster.PlayerState.Options.ShufflingContext);
                        }
                    }

                    if (Math.Abs(_currentPosition - GetPosition(update.Cluster)) > 10)
                    {
                        var curPos = GetPosition(update.Cluster);
                        _spotifyConnectClient.OnPositionChanged(this, curPos);
                        _currentPosition = curPos;
                    }
                }

                //var now = TimeProvider.CurrentTimeMillis();
                //var ts = update.Cluster.Timestamp - 3000; // Workaround
                //if (!_spotifyConnectClient.Client.Config.DeviceId.Equals(update.Cluster.ActiveDeviceId)
                //    && _requestState.IsActive
                //    && now > (long)_requestState.PutStateRequest.StartedPlayingAt
                //    && ts > (long)_requestState.PutStateRequest.StartedPlayingAt)
                //{
                //    foreach (var dealerClientReqListener in _dealerClient.ReqListeners)
                //    {
                //        dealerClientReqListener.Value.NotActive();
                //    }
                //}

                PreviousCluster = update.Cluster; 
                return Task.CompletedTask;
            }

            return Task.FromResult($"Could not handle uri {uri}");
        }
        internal int GetPosition(Cluster cluster)
        {
            int diff = (int)(TimeProvider.CurrentTimeMillis() - cluster.PlayerState.Timestamp);
            return (int)(cluster.PlayerState.PositionAsOfTimestamp + diff);
        }
        private static RepeatState ParseRepeatState(Cluster cluster)
        {
            var repeatingTrack = cluster.PlayerState.Options.RepeatingTrack;
            var repeatingContext = cluster.PlayerState.Options.RepeatingContext;
            if (repeatingContext && !repeatingTrack)
            {
                return RepeatState.Context;
            }
            else
            {
                if (repeatingTrack)
                {
                    return RepeatState.Track;
                }
                else
                {
                    return RepeatState.Off;
                }
            }
        }
        private double _currentPosition;

    }
}
