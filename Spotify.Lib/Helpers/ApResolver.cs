﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;

namespace Spotify.Lib.Helpers
{
    public static class ApResolver
    {
        private static readonly HttpClient HttpClient = new();
        private static string _resolvedDealer;
        private static string _resolvedSpClient;
        private static List<(string host, int port)> _resolvedAccessPoint;

        public static async Task<string> GetClosestDealerAsync()
        {
            if (!string.IsNullOrEmpty(_resolvedDealer))
                return _resolvedDealer;
            //https://apresolve.spotify.com/?type=dealer
            var dealers = await HttpClient.GetStringAsync("https://apresolve.spotify.com/?type=dealer");
            var x = JObject.Parse(dealers)["dealer"];
            _resolvedDealer = "https://" + x.First;
            return _resolvedDealer;
        }

        public static async Task<string> GetClosestSpClient()
        {
            if (!string.IsNullOrEmpty(_resolvedSpClient))
                return _resolvedSpClient;
            //https://apresolve.spotify.com/?type=spclient
            var spclients = await HttpClient.GetStringAsync("https://apresolve.spotify.com/?type=spclient");
            var x = JObject.Parse(spclients)["spclient"];
            _resolvedSpClient = "https://" + x.First;
            return _resolvedSpClient;
        }

        public static async Task<(string host, int port)> GetClosestAccessPoint()
        {
            //https://apresolve.spotify.com/?type=spclient

            var spClients = await "http://apresolve.spotify.com/?type=accesspoint".GetJsonAsync<AccessPoints>();
            return spClients.accesspoint.Select(host =>
                (host.Split(':')[0], int.Parse(host.Split(':')[1]))).FirstOrDefault();
        }
    }


    public class AccessPoints
    {
        public string[] accesspoint { get; set; }
    }
}