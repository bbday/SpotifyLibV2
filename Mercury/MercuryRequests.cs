﻿using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Spotify;
using SpotifyLibV2.Models.Public;
using SpotifyLibV2.Models.Response;
using SpotifyLibV2.Models.Response.MercuryContext;

namespace SpotifyLibV2.Mercury
{
    public static class MercuryRequests
    {
        private static readonly string KEYMASTER_CLIENT_ID = "65b708073fc0480ea92a077233ca87bd";
        public static JsonMercuryRequest<MercuryContextWrapperResponse> ResolveContext([NotNull] string uri)
        {
            return new(RawMercuryRequest.Get(
                $"hm://context-resolve/v1/{uri}"));
        }
        public static JsonMercuryRequest<StoredToken> RequestToken([NotNull] string deviceId, [NotNull] string[] scope)
        {
            return new(RawMercuryRequest.Get(
                $"hm://keymaster/token/authenticated?scope={string.Join(",", scope)}&client_id={KEYMASTER_CLIENT_ID}&device_id={deviceId}"));
        }

        public static JsonMercuryRequest<string> GetGenericJson([NotNull] string uri)
        {
            return new(RawMercuryRequest.Get(uri));
        }

        // public static JsonMercuryRequest<MercuryArtist> GetArtist([NotNull] ArtistId id)
        // {
        //    return new JsonMercuryRequest<MercuryArtist>(RawMercuryRequest.Get(id.ToMercuryUri()));
        //}
        public static RawMercuryRequest AutoplayQuery([NotNull] string context)
        {
            return RawMercuryRequest.Get("hm://autoplay-enabled/query?uri=" + context);
        }
        public static JsonMercuryRequest<string> GetStationFor([NotNull] string context)
        {
            return new(RawMercuryRequest.Get("hm://radio-apollo/v3/stations/" + context));
        }
        public static ProtobuffedMercuryRequest<MercuryMultiGetReply> MultiGet(string uri, IEnumerable<MercuryRequest> requests)
        {
            var request = new RawMercuryRequest(
                    uri,
                    "GET",
                    requests)
                .ContentType("vnd.spotify/mercury-mget-request");
            return new ProtobuffedMercuryRequest<MercuryMultiGetReply>(request, MercuryMultiGetReply.Parser);
        }
    }
}
