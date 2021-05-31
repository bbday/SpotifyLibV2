using System;
using Spotify.Lib.Connect;
using Spotify.Lib.Interfaces;
using SpotifyProto;

namespace Spotify.Lib.Models
{
    public readonly struct TrackOrEpisode : IEquatable<TrackOrEpisode>
    {
        public readonly ISpotifyId id;
        public readonly Track track;
        public readonly Episode episode;

        public TrackOrEpisode(Track track, Episode episode)
        {
            if (track == null && episode == null)
            {
                this.track = null;
                this.episode = null;
                id = null;
                return;
            };

            this.track = track;
            this.episode = episode;

            if (track != null) id = PlayableId.From(track);
            else id = PlayableId.From(episode);
        }

        internal TrackOrEpisode(object? metadata)
        {
            switch (metadata)
            {
                case Track tr:
                    track = tr;
                    episode = null;
                    id = PlayableId.From(tr);
                    return;
                case Episode ep:
                    episode = ep;
                    track = null;
                    id = PlayableId.From(ep);
                    return;
                default:
                    throw new Exception();
                    break;
            }
        }

        public int Duration()
        {
            return track?.Duration ?? episode.Duration;
        }

        public string Name => track?.Name ?? episode?.Name;

        public bool Equals(TrackOrEpisode other)
        {
            return id.Equals(other.id);
        }

        public override bool Equals(object? obj)
        {
            return obj is TrackOrEpisode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}