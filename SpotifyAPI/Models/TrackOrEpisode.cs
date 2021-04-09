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

        public TrackOrEpisode(object? metadata)
        {
            switch (metadata)
            {
                case Track tr:
                    this.track = tr;
                    this.episode = null;
                    id = PlayableId.From(tr);
                    return;
                case Episode ep:
                    this.episode = ep;
                    this.track = null;
                    id = PlayableId.From(ep);
                    return;
                default:
                    id = null;
                    track = null;
                    episode = null;
                    break;
            }
        }

        public int Duration() => track?.Duration ?? episode.Duration;
        public string Name => track?.Name ?? episode?.Name;
    }
}
