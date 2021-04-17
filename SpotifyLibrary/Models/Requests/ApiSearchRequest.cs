﻿using MediaLibrary.Enums;
using Refit;

namespace SpotifyLibrary.Models.Requests
{
    public readonly struct ApiSearchRequest
    {
        public ApiSearchRequest(string query)
        {
            Query = query;
            Type = AudioItemType.Track.ToString();
            DecorateRestrictions = false;
            Limit = 50;
            Offset = 0;
            Market = "from_token";
            IncludeExternal = "audio";
        }
        public ApiSearchRequest(string q,
            int offset = 0,
            int limit = 50,
            params AudioItemType[] types) 
        {
            Query = q;
            Type = string.Join(",", types).ToLowerInvariant();
            Offset = offset;
            Limit = limit;
            Market = "from_token";

            DecorateRestrictions = false;
            IncludeExternal = "audio";
        }
        [AliasAs("type")]
        public string Type { get;  }
        [AliasAs("q")]
        public string Query { get; }
        [AliasAs("decorate_restrictions")]
        public bool DecorateRestrictions { get; }
        [AliasAs("include_external")] 
        public string IncludeExternal { get; }
        [AliasAs("limit")]
        public int Limit { get; }
        [AliasAs("offset")]
        public int Offset { get; }

        [AliasAs("market")] 
        public string Market { get; }
    }
}
