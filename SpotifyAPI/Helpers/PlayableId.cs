using System;
using System.Collections.Generic;
using System.Linq;
using Base62;
using Connectstate;
using JetBrains.Annotations;
using Spotify.Player.Proto;
using SpotifyLibrary.Models.Ids;
using SpotifyProto;

namespace SpotifyLibrary.Helpers
{
    public static class PlayableId
    {
        public static string InferUriPrefix(string contextUri)
        {
            if (contextUri.StartsWith("spotify:episode:") || contextUri.StartsWith("spotify:show:"))
                return "spotify:episode:";
            return "spotify:track:";
        }

        public static bool CanPlaySomething([NotNull] List<ContextTrack> tracks)
        {
            return tracks.Any(x => IsSupported(x.Uri) && ShouldPlay(x));
        }

        public static bool ShouldPlay([NotNull] ContextTrack track)
        {
            string forceRemoveReasons = null;
            if (track.Metadata.ContainsKey("force_remove_reasons"))
                forceRemoveReasons = track.Metadata["force_remove_reasons"];
            return forceRemoveReasons == null || string.IsNullOrEmpty(forceRemoveReasons);
        }

        public static bool IsSupported([NotNull] string uri)
        {
            return !uri.StartsWith("spotify:local:") && !Equals(uri, "spotify:delimiter")
                                                     && !Equals(uri, "spotify:meta:delimiter");
        }

        public static ISpotifyId From([NotNull] ContextTrack track)
        {
            if (track.Uri.Contains("episode"))
                return new EpisodeId(track.Uri);
            return new TrackId(track.Uri);
        }

        public static ISpotifyId From([NotNull] ProvidedTrack track)
        {
            if (track.Uri.Contains("episode"))
                return new EpisodeId(track.Uri);
            return new TrackId(track.Uri);
        }

        public static ISpotifyId FromUri([NotNull] string uri)
        {
            if (!IsSupported(uri)) throw new Exception("Unsupported id.");

            if (uri.Split(':')[1] == "track") return new TrackId(uri);
            if (uri.Split(':')[1] == "episode")
                return new EpisodeId(uri);
            throw new Exception("Unknown uri: " + uri);
        }

        public static ISpotifyId From([NotNull] Track track)
        {
            return new TrackId($"spotify:track:{EncodingExtensions.ToBase62(track.Gid.ToByteArray(), true)}");
        }

        public static ISpotifyId From([NotNull] Episode track)
        {
            return new TrackId($"spotify:episode:{EncodingExtensions.ToBase62(track.Gid.ToByteArray(), true)}");
        }
    }
}