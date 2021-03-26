using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using SpotifyLibrary.Dealer;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyMessageState : IMessageListener
    {
        private readonly DealerClient _dealerClient;
        private readonly SpotifyConnectClient _spotifyConnectClient;
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

        public Task OnMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            if (uri.StartsWith("hm://track-playback/v1/command"))
            {
                Debugger.Break();
            }
            else if (uri.StartsWith("hm://pusher/v1/connections/"))
            {
                _spotifyConnectClient.UpdateConnectionId(headers["Spotify-Connection-Id"]);
            }
            else if (uri.StartsWith("hm://connect-state/v1/connect/volume"))
            {

            }
            else if (uri.StartsWith("hm://connect-state/v1/cluster"))
            {
                var update = ClusterUpdate.Parser.ParseFrom(payload);
                _spotifyConnectClient.OnNewPlaybackWrapper(update.Cluster);
            }
            else
            {
                Debug.WriteLine($"Message left unhandled! uri {uri}");
            }

            return Task.CompletedTask;
        }
    }
}
