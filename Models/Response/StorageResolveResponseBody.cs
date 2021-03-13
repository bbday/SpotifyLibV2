using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
	/// <summary>
    /// Endpoints for resolving the URLs to encrypted Spotify audio.
    /// </summary>
    public class StorageResolveResponseBody
    {
        /// <summary>
        /// The endpoints where the file can be played from.
        /// </summary>
        /// <remarks>
        /// First item item is adaptive playback, second is legacy
        /// </remarks>
        [JsonProperty("cdnurl")]
        public IEnumerable<string> CdnUrls { get; set; }

        /// <summary>
        /// The ID of the resolved file
        /// </summary>
        [JsonProperty("fileid")]
        public string FileId { get; set; }

        /// <summary>
        /// Details unknown.
        /// </summary>
        /// <remarks>
        /// Known values: <example><c>CDN</c></example>
        /// </remarks>
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
