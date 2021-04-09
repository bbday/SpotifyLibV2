using System;
using System.Net.Http;
using MusicLibrary.Enum;
using SpotifyLibrary.Models;

namespace SpotifyLibrary
{
    internal delegate void HttpMessageReceived(HttpResponseMessage message);

    internal delegate void MercuryConnectionEstablished(TimeSpan timetaken);

    internal delegate void MercuryConnectionDisconnected(DateTime startedAt, DateTime endedAt,
        ConnectionDroppedReason reason);

    public delegate void OnNewCluster(Connectstate.ClusterUpdate clusterupdate);

    public delegate void PlayerStateChanged(MediaPlaybackState state, TrackOrEpisode metadata);
}