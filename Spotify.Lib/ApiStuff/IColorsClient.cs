using System;
using System.Threading.Tasks;
using Refit;
using Spotify.Lib.Attributes;
using Spotify.Lib.Models.Response;

namespace Spotify.Lib.ApiStuff
{

    [BaseUrl("https://api-partner.spotify.com")]
    public interface IColorsClient
    {
        [Get(
            "/pathfinder/v1/query?operationName=fetchExtractedColor&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22ad7208355ce3e5ed223621bf31cb8a66b239cd387d0d2ec0ff1ce99310f6d974%22%7D%7D")]
        Task<PathFinderParser<PathfinderColors>> GetColors(ColorsRequest r);

        [Get("/pathfinder/v1/query?operationName=queryFullscreenMode&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22a90a0143ba80bf9d08aa03c61c86d33d214b9871b604e191d3a63cbb2767dd20%22%7D%7D")]
        Task<PathFinderParser<PathfinderFullScreen>> GetFullscreen(FullscreenModeRequest s);
    }

    public readonly struct FullscreenModeRequest
    {
        public FullscreenModeRequest(string artistUri)
        {
            Variables = "{\"artistUri\":\"" + artistUri + "\"}";
        }
        [AliasAs("variables")]
        public string Variables { get; }
    }
    public readonly struct ColorsRequest
    {
        public ColorsRequest(Uri imageUri)
        {
            Variables = "{\"uri\":\"" + imageUri + "\"}";
        }
        [AliasAs("variables")]
        public string Variables { get; }
    }
}

