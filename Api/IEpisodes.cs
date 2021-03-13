using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IEpisodes
    {
        /// <summary>
        /// Get Spotify catalog information for a single episode identified by its unique Spotify ID.
        /// </summary>
        /// <param name="episodeId">The Spotify ID for the episode.</param>
        /// <remarks>
        /// https://developer.spotify.com/documentation/web-api/reference-beta/#endpoint-get-an-episode
        /// </remarks>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
        [Get("/v1/episodes/{episodeId}")]
        Task<FullEpisode> Get(string episodeId);
    }
}
