using System;
using System.Collections.Generic;
using System.Text;
using Refit;

namespace SpotifyLibrary.Models.Requests
{
    public class ArtistsRequest 
    {
        /// <summary>
        /// ArtistsRequest
        /// </summary>
        /// <param name="ids">A comma-separated list of the Spotify IDs for the artists. Maximum: 50 IDs.</param>
        public ArtistsRequest(IList<string> ids)
        {
            Ids = ids;
        }

        /// <summary>
        /// A comma-separated list of the Spotify IDs for the artists. Maximum: 50 IDs.
        /// </summary>
        /// <value></value>
        [AliasAs("ids")]
        public IList<string> Ids { get; }
    }
}

