using System;
using System.Collections.Generic;
using System.Text;
using Refit;

namespace SpotifyLibV2.Models.Request
{
    public class LibraryCheckTracksRequest 
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="ids">
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </param>
        public LibraryCheckTracksRequest(IList<string> ids)
        {
            Ids = ids;
        }

        /// <summary>
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </summary>
        /// <value></value>
        [AliasAs("ids")]
        public IList<string> Ids { get; }
    }
}
