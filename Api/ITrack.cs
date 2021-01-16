using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface ITrack
    {
        /// <summary>
        /// Get Spotify catalog information for a single track identified by its unique Spotify ID.
        /// </summary>
        /// <param name="trackId">The Spotify ID for the track.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        [Get("/v1/tracks/{trackId}")]
        Task<FullTrack> GetTrack(string trackId, [AliasAs("market")] string market = "from_token");
    }
}
