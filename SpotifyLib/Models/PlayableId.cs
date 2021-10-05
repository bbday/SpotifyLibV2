using System;
using System.Collections.Generic;
using System.Linq;
using Base62;
using Connectstate;
using Spotify.Metadata.Proto;
using Spotify.Player.Proto;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models
{
    public static class PlayableId
    {
        public static string InferUriPrefix(string contextUri)
        {
            if (contextUri.StartsWith("spotify:episode:") || contextUri.StartsWith("spotify:show:"))
                return "spotify:episode:";
            return "spotify:track:";
        }

        public static bool CanPlaySomething(List<ContextTrack> tracks)
        {
            return tracks.Any(x => IsSupported(x.Uri) && ShouldPlay(x));
        }

        public static bool ShouldPlay(ContextTrack track)
        {
            string forceRemoveReasons = null;
            if (track.Metadata.ContainsKey("force_remove_reasons"))
                forceRemoveReasons = track.Metadata["force_remove_reasons"];
            return forceRemoveReasons == null || string.IsNullOrEmpty(forceRemoveReasons);
        }

        public static bool IsSupported(string uri)
        {
            return !uri.StartsWith("spotify:local:") && !Equals(uri, "spotify:delimiter")
                                                     && !Equals(uri, "spotify:meta:delimiter");
        }

        public static SpotifyId From(ContextTrack track)
        {
            return new SpotifyId(track.Uri);
        }

        public static SpotifyId From(ProvidedTrack track) => track.Uri.IsEmpty() ? new SpotifyId() : 
            new SpotifyId(track.Uri);


        public static SpotifyId From(Track track)
        {
            return new SpotifyId($"spotify:track:{track.Gid.ToByteArray().ToBase62(true)}");
        }

        public static SpotifyId From(Episode track)
        {
            return new SpotifyId($"spotify:episode:{track.Gid.ToByteArray().ToBase62(true)}");
        }
    }
}