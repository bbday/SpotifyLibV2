using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyMessageState : IMessageListener
    {
        private readonly DealerClient _dealerClient;
        private readonly SpotifyConnectClient _spotifyConnectClient;
        private ClusterUpdate _previousCluster;
        private RepeatState? _previousRepeatState;

        internal SpotifyMessageState(DealerClient dealerClient,
            SpotifyConnectClient onNewCluster)
        {
            _dealerClient = dealerClient;
            _spotifyConnectClient = onNewCluster;
            dealerClient.AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
        }

        private double _currentPosition;
        public async Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            if (uri.StartsWith("hm://track-playback/v1/command"))
            {
                Debugger.Break();
            }
            else if (uri.StartsWith("hm://pusher/v1/connections/"))
            {
               await _spotifyConnectClient.UpdateConnectionId(headers["Spotify-Connection-Id"]);
            }
            else if (uri.StartsWith("hm://connect-state/v1/connect/volume"))
            {

            }
            else if (uri.StartsWith("hm://connect-state/v1/cluster"))
            {
                var update = ClusterUpdate.Parser.ParseFrom(payload);
                // _spotifyConnectClient.OnNewPlaybackWrapper(this, update.Cluster);
                if (update.Cluster?.PlayerState != null)
                {
                    if (update.Cluster.PlayerState?.Track?.Uri != null)
                    {
                        if (_previousCluster?.Cluster?.PlayerState?.Track?.Uri
                            != update.Cluster.PlayerState.Track.Uri)
                        {
                            _spotifyConnectClient.OnNewPlaybackWrapper(this, update.Cluster.PlayerState);
                        }
                    }
                    if (_previousCluster?.Cluster?.PlayerState?.Options == null)
                    {
                        _spotifyConnectClient.OnShuffleStatecHanged(this, update.Cluster.PlayerState.Options.ShufflingContext);
                        _previousRepeatState = ParseRepeatState(update.Cluster);
                        _spotifyConnectClient.OnRepeatStateChanged(this, _previousRepeatState.Value);
                    }
                    else
                    {
                        var newRepeatState = ParseRepeatState(update.Cluster);
                        if (_previousRepeatState != newRepeatState)
                        {
                            _spotifyConnectClient.OnRepeatStateChanged(this, newRepeatState);
                            _previousRepeatState = newRepeatState;
                        }

                        if (_previousCluster.Cluster.PlayerState.Options.ShufflingContext !=
                            update.Cluster.PlayerState.Options.ShufflingContext)
                        {
                            _spotifyConnectClient.OnShuffleStatecHanged(this, update.Cluster.PlayerState.Options.ShufflingContext);
                        }
                    }

                    if (Math.Abs(_currentPosition - GetPosition(update.Cluster)) > 10)
                    {
                        var curPos = GetPosition(update.Cluster);
                        _spotifyConnectClient.OnPositionChanged(this, curPos);
                        _currentPosition = curPos;
                    }
                }


                _previousCluster = update;
            }
            else
            {
                Debug.WriteLine($"Message left unhandled! uri {uri}");
            }
        }

        public int GetPosition(Cluster cluster)
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
    }
}
