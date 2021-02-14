using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [BaseUrl("https://api-partner.spotify.com")]
    public interface IPathFinder
    {

        [Get(
            "/pathfinder/v1/query?operationName=fetchExtractedColor&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22ad7208355ce3e5ed223621bf31cb8a66b239cd387d0d2ec0ff1ce99310f6d974%22%7D%7D")]
        Task<PathFinderParser<PathfinderColors>> GetColors([AliasAs("variables")] string parameters);


        [Get(
            "/pathfinder/v1/query?operationName={commandName}&variables={parameters}&extensions=%7B%22persistedQuery%22%3A%7B%22version%22%3A1%2C%22sha256Hash%22%3A%22bc11db3d3a456e7f2d75ee30c81d61ef6d8697716a57906344f02c15425eab3a%22%7D%7D")]
        Task<PathFinderParser<T>> GetGenericCommand<T>(string parameters, string commandName);
    }
}
