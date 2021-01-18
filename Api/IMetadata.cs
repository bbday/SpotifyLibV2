using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    [ResolvedSpClientEndpoint]
    public interface IMetadata
    {
        [Get("/playlist/v2/playlist/{plistId}/metadata")]
        Task<PlaylistMetadataProto> GetMetadataForPlaylist(string plistId);
    }
}
