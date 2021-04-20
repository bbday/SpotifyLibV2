using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;

namespace SpotifyLibrary.Helpers
{
    public static class PlayCommandHelper
    {
        public static bool? IsInitiallyPaused( JObject obj)
        {
            var options = obj["options"];
            if (options == null) return null;

            JToken elm;
            if ((elm = options["initially_paused"]) != null) return elm.ToObject<bool>();
            else return null;
        }

        public static string GetContextUri(JObject obj)
        {
            var context = obj["context"];
            if (context == null) return null;

            JToken elm;
            return (elm = context["uri"]) != null ? elm.ToObject<string>() : null;
        }
        public static JToken GetPlayOrigin( JObject obj) => obj["play_origin"];
        public static JToken GetContext( JObject obj) => obj["context"];

        public static JToken GetPlayerOptionsOverride( JObject obj) =>
            obj["options"]?["player_options_override"];
        public static bool WillSkipToSomething( JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return false;

            parent = parent["skip_to"];
            if (parent == null) return false;

            var checks = new[]
            {
                "track_uid",
                "track_uri",
                "track_index"
            };
            return checks.Any(check => parent[check] != null);
        }
        public static string GetSkipToUid( JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return null;

            parent = parent["skip_to"];
            if (parent == null) return null;

            JToken elm;
            return (elm = parent["track_uid"]) != null ? elm.ToObject<string>() : null;
        }
        public static int? GetSkipToIndex( JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return null;

            parent = parent["skip_to"];
            if (parent == null) return null;

            JToken elm;
            if ((elm = parent["track_index"]) != null) return elm.ToObject<int>();
            return null;
        }

        public static string GetSkipToUri( JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return null;

            parent = parent["skip_to"];
            if (parent == null) return null;

            JToken elm;
            return (elm = parent["track_uri"]) != null ? elm.ToObject<string>() : null;
        }

        public static List<ContextTrack> GetNextTracks( JObject obj)
        {
            if (!(obj["next_tracks"] is JArray prevTracks)) return null;
            return ProtoUtils.JsonToContextTracks(prevTracks);
        }

        public static ContextTrack GetTrack( JObject obj)
        {
            var track = obj["track"];
            return track == null ? null : ProtoUtils.JsonToContextTrack(track);
        }
        public static int? GetSeekTo( JObject obj)
        {
            var options = obj["options"];
            if (options == null) return null;

            JToken elm;
            if ((elm = options["seek_to"]) != null) return elm.ToObject<int>();
            return null;
        }
    }
}