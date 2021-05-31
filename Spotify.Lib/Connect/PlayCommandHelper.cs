using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Spotify.Lib.Connect
{
    internal static class PlayCommandHelper
    {
        internal static string GetContextUri(JObject obj)
        {
            var context = obj["context"];
            return context?["uri"]?.ToString();
        }

        public static JObject GetPlayOrigin(JObject obj) => obj["play_origin"] as JObject;
        public static JObject GetContext(JObject obj) => obj["context"] as JObject;
        public static JObject GetPlayerOptionsOverride(JObject obj) =>
            obj["options"]?["player_options_override"] as JObject;

        public static string GetSkipToUid(JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return null;

            parent = parent["skip_to"];
            if (parent == null) return null;

            JToken elm;
            return (elm = parent["track_uid"]) != null ? elm.ToObject<string>() : null;
        }
        public static string GetSkipToUri(JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return null;

            parent = parent["skip_to"];
            if (parent == null) return null;

            JToken elm;
            return (elm = parent["track_uri"]) != null ? elm.ToObject<string>() : null;
        }

        public static int? GetSkipToIndex(JObject obj)
        {
            var parent = obj["options"];
            if (parent == null) return null;

            parent = parent["skip_to"];
            if (parent == null) return null;

            JToken elm;
            if ((elm = parent["track_index"]) != null) return elm.ToObject<int>();
            return null;
        }

        public static int? GetSeekTo(JObject obj)
        {
            var options = obj["options"];
            if (options == null) return null;

            JToken elm;
            if ((elm = options["seek_to"]) != null) return elm.ToObject<int>();
            return null;
        }

        public static bool? IsInitiallyPaused(JObject obj)
        {
            var options = obj["options"];
            if (options == null) return null;

            JToken elm;
            if ((elm = options["initially_paused"]) != null) return elm.ToObject<bool>();
            else return null;
        }
    }
}
