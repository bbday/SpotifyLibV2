using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using MoreLinq.Extensions;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models.Queue;
using SpotifyLibrary.Connect.Enums;
using SpotifyLibrary.Dealer;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyMessageState : IMessageListener
    {
        private readonly DealerClient _dealerClient;
        private readonly SpotifyConnectClient _spotifyConnectClient;

        public Cluster PreviousCluster
        {
            get; internal set;
        }
        private RepeatState? _previousRepeatState;
        private readonly SpotifyRequestState _requestState;
        internal SpotifyMessageState(DealerClient dealerClient,
            SpotifyRequestState requestState,
            SpotifyConnectClient onNewCluster)
        {
            _dealerClient = dealerClient;
            _requestState = requestState;
            _spotifyConnectClient = onNewCluster;
            dealerClient.AddMessageListener(this,
                "hm://pusher/v1/connections/",
                "hm://connect-state/v1/connect/volume",
                "hm://connect-state/v1/cluster");
        }

        private double _currentPosition;
        public string CurrentDeviceId => PreviousCluster?.ActiveDeviceId;
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
                        if (PreviousCluster?.PlayerState?.Track?.Uri
                            != update.Cluster.PlayerState.Track.Uri)
                        {
                            _spotifyConnectClient.OnNewPlaybackWrapper(this, update.Cluster.PlayerState);
                        }
                    }
                    if(PreviousCluster?.PlayerState != null && update.Cluster?.PlayerState != null)
                    {
                        if(PreviousCluster.PlayerState.IsPaused != update.Cluster.PlayerState.IsPaused)
                        {
                            if (!update.Cluster.PlayerState.IsPaused)
                            {
                                _spotifyConnectClient.OnPlaybackStateChanged(this,
                                    MediaPlaybackState.TrackStarted);
                            }
                            else
                            {
                                _spotifyConnectClient.OnPlaybackStateChanged(this,
                                    MediaPlaybackState.TrackPaused);
                            }
                        }

                        var playerState = update.Cluster.PlayerState;
                        var q = playerState.NextTracks.Where(z =>
                            z.Provider == "queue")
                            .ToList();
                        var qbf = 
                            PreviousCluster.PlayerState.NextTracks.Where(z =>
                            z.Provider == "queue")
                                .ToList();

                        var queueUpdates = new List<IQueueUpdateItem>();
                        if (playerState.Track != null)
                        {
                            var currentlyPlaying = PlayableId.From(playerState.Track);
                            foreach (var qAfter in q)
                            {
                                var indexOf = q.IndexOf(qAfter);

                                var tryAndGetInBefore = qbf.Exists(z => z.Uid == qAfter.Uid);
                                if (!tryAndGetInBefore)
                                {
                                    var id = PlayableId.From(qAfter);
                                    queueUpdates.Add(new QueueAddedItem(id,
                                        qAfter.Metadata,
                                        indexOf));
                                }
                                else
                                {
                                    var indexBefore = qbf.FindIndex(z => z.Uid == qAfter.Uid);
                                    if (indexBefore != indexOf)
                                    {
                                        var id = PlayableId.From(qAfter);
                                        queueUpdates.Add(new QueueMovedItem(id,
                                            qAfter.Metadata, indexBefore, indexOf));
                                    }
                                }
                            }
                        }
                        //foreach (var qBefore in qbf)
                        //{
                        //    var tryAndGetInAfter = 
                        //        q.Exists(z => z.Uid == qBefore.Uid);
                        //    if (!tryAndGetInAfter)
                        //    {
                        //        var id = PlayableId.From(qBefore);
                        //        if (!id.Equals(currentlyPlaying))
                        //        {
                        //            var indexOf = qbf.IndexOf(qBefore);
                        //            queueUpdates.Add(new QueueRemovedItem(id, qBefore.Metadata, indexOf));
                        //        }
                        //    }
                        //}


                        if (queueUpdates.Any())
                        {
                            _spotifyConnectClient.OnNewItemsInQueue(this, queueUpdates);
                        }
                    }
                    if (PreviousCluster?.PlayerState?.Options == null)
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

                        if (PreviousCluster.PlayerState.Options.ShufflingContext !=
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

                var now = TimeProvider.CurrentTimeMillis();
                var ts = update.Cluster.Timestamp - 3000; // Workaround
                if (!_spotifyConnectClient.Client.Config.DeviceId.Equals(update.Cluster.ActiveDeviceId)
                    && _requestState.IsActive
                    && now > (long) _requestState.PutStateRequest.StartedPlayingAt
                    && ts > (long) _requestState.PutStateRequest.StartedPlayingAt)
                {
                    foreach (var dealerClientReqListener in _dealerClient.ReqListeners)
                    {
                        dealerClientReqListener.Value.NotActive();
                    }
                }

                PreviousCluster = update.Cluster;
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
