using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace SpotifyLib.Helpers
{
    public static class ApResolver
    {
        private static string _resolvedSpClient;

        public static async Task<IEnumerable<(string, int)>> GetClosestAccessPoint(CancellationToken ct)
        {
            var spClients = await "http://apresolve.spotify.com/?type=accesspoint"
                .GetJsonAsync<AccessPoints>(ct);
            return spClients.accesspoint.Select(host =>
                (host.Split(':')[0], int.Parse(host.Split(':')[1])));
        }
        private readonly struct AccessPoints
        {
            public AccessPoints(string[] accesspoint)
            {
                this.accesspoint = accesspoint;
            }

            public string[] accesspoint { get; }
        }

        public static async Task<string> GetClosestDealerAsync()
        {
            //https://apresolve.spotify.com/?type=dealer
            var dealers = await "https://apresolve.spotify.com/?type=dealer"
                .GetStringAsync();
            var x = JObject.Parse(dealers)["dealer"];
            return "https://" + x.First;
        }

        public static async Task<string> GetClosestSpClient()
        {
            if (!string.IsNullOrEmpty(_resolvedSpClient))
                return _resolvedSpClient;
            //https://apresolve.spotify.com/?type=spclient
            var spclients = await "https://apresolve.spotify.com/?type=spclient"
                .GetStringAsync();
            var x = JObject.Parse(spclients)["spclient"];
            _resolvedSpClient = "https://" + x.First;
            return _resolvedSpClient;
        }
    }
}
