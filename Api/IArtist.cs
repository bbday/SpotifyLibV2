using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLib.Api
{
    [BaseUrl("https://api.spotify.com")]
    public interface IArtist
    {
        [Get("/v1/artists/{id}")]
        Task<FullArtist> GetArtist(string id);
    }
}
