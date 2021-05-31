using System.Collections.Generic;
using Refit;

namespace Spotify.Lib.Models.Request
{
    public readonly struct TracksRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids">A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.</param>
        /// <param name="fromToken"></param>
        public TracksRequest(IList<string> ids, string market = null)
        {
            Ids = ids;
            Market = market;
        }

        /// <summary>
        /// A comma-separated list of the Spotify IDs for the tracks. Maximum: 50 IDs.
        /// </summary>
        /// <value></value>
        [AliasAs("ids")]
        public IList<string> Ids { get; }

        /// <summary>
        /// An ISO 3166-1 alpha-2 country code or the string from_token.
        /// Provide this parameter if you want to apply Track Relinking.
        /// </summary>
        /// <value></value>
        [AliasAs("market")]
        public string? Market { get; }
    }
}