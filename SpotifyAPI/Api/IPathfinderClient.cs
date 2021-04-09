using System.Threading.Tasks;
using Refit;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Pathfinder;

namespace SpotifyLibrary.Api
{
    [BaseUrl("https://api-partner.spotify.com")]
    public interface IPathfinderClient
    {
        [Get(
            "/pathfinder/v1/query?operationName=fetchExtractedColor&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22ad7208355ce3e5ed223621bf31cb8a66b239cd387d0d2ec0ff1ce99310f6d974%22%7D%7D")]
        Task<PathFinderParser<PathfinderColors>> GetColors([AliasAs("variables")] string parameters);

        [Get(
            "/pathfinder/v1/query?operationName=queryAlbumTracks&variables=%7B%22uri%22%3A%22{uri}%22%2C%22offset%22%3A0%2C%22limit%22%3A300%7D&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%223ea563e1d68f486d8df30f69de9dcedae74c77e684b889ba7408c589d30f7f2e%22%7D%7D")]
        Task<PathFinderParser<PathfinderAlbum>> GetAlbumTracks(string uri);
    }
}
