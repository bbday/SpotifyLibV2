using System;
using JetBrains.Annotations;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models.Ids;
using SpotifyProto;

namespace SpotifyLibrary.Models
{
    public readonly struct TrackOrEpisode
    {
        public readonly ISpotifyId id;
        public readonly Track track;
        public readonly Episode episode;

        public TrackOrEpisode([CanBeNull] Track track, [CanBeNull] Episode episode)
        {
            if (track == null && episode == null) throw new ArgumentOutOfRangeException();

            this.track = track;
            this.episode = episode;

            if (track != null) id = PlayableId.From(track);
            else id = PlayableId.From(episode);
        }

        public int Duration() => track?.Duration ?? episode.Duration;
    }
}
