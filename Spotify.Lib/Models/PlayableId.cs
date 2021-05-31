using System;
using System.Collections.Generic;
using System.Linq;
using Base62;
using Connectstate;
using Spotify.Lib.Interfaces;
using Spotify.Lib.Models.Ids;
using Spotify.Player.Proto;
using SpotifyProto;

namespace Spotify.Lib.Models
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

        public static ISpotifyId From(ContextTrack track)
        {
            if (track.Uri.Contains("episode"))
                return new EpisodeId(track.Uri);
            return new TrackId(track.Uri);
        }

        public static ISpotifyId From(ProvidedTrack track)
        {
            if (track.Uri.Contains("episode"))
                return new EpisodeId(track.Uri);
            return new TrackId(track.Uri);
        }

        public static ISpotifyId FromUri(string uri)
        {
            if (!IsSupported(uri))
                throw new Exception("Unsupported id.");

            if (uri.Split(':')[1] == "track") return new TrackId(uri);
            if (uri.Split(':')[1] == "episode")
                return new EpisodeId(uri);
            throw new Exception("Unknown uri: " + uri);
        }

        public static ISpotifyId From(Track track)
        {
            return new TrackId($"spotify:track:{track.Gid.ToByteArray().ToBase62(true)}");
        }

        public static ISpotifyId From(Episode track)
        {
            return new TrackId($"spotify:episode:{track.Gid.ToByteArray().ToBase62(true)}");
        }
    }
}