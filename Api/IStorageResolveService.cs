using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using SpotifyLibV2.Attributes;
using SpotifyLibV2.Models.Response;

namespace SpotifyLibV2.Api
{
    /// <summary>
    /// Resolves
    /// </summary>
    [ResolvedSpClientEndpoint]
    public interface IStorageResolveService
    {
        /// <summary>
        /// Resolves the playback endpoints for a fileId.
        /// </summary>
        /// <param name="fileId">
        /// The file ID of the track.
        /// </param>
        /// <returns>
        /// <placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder>
        /// </returns>
        [Get("/storage-resolve/files/audio/interactive/{fileId}?version=10000000&product=9&platform=39&alt=json")]
        Task<StorageResolveResponseBody> ResolveFile(
            [AliasAs("fileId")] string fileId);
    }
}
