using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Ids;
using SpotifyProto;

namespace SpotifyLibrary.Models
{
    public readonly struct TrackOrEpisode : IEquatable<TrackOrEpisode>
    {
        public readonly ISpotifyId id;
        public readonly Track track;
        public readonly Episode episode;

        public TrackOrEpisode(Track track, Episode episode)
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
                    throw new Exception();
                    break;
            }
        }

        public int Duration() => track?.Duration ?? episode.Duration;
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
