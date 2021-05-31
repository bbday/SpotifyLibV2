using System;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Ids
{
    public abstract class StandardIdEquatable<T> : ISpotifyId where T : ISpotifyId
    {
        protected StandardIdEquatable(string uri, string id,
            AudioItemType type)
        {
            AudioType = type;
            Id = id;
            Uri = uri;
        }

        public AudioItemType AudioType { get; set; }
        public string Uri { get; set; }
        public string Id { get; set; }

        public abstract string ToMercuryUri(string locale);
        public abstract string ToHexId();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((T) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int) AudioType, Uri);
        }

        public virtual bool Equals(ISpotifyId other)
        {
            if (other is T genid) return genid.Uri == Uri;
            return false;
        }

        public override string ToString()
        {
            return Uri;
        }
    }
}