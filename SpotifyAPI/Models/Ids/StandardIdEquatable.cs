using System;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;

namespace SpotifyLibrary.Models.Ids
{
    public abstract class StandardIdEquatable<T> : ISpotifyId where T : ISpotifyId
    {
        protected StandardIdEquatable(string uri, string id,
            AudioType type,
            AudioService service)
        {
            AudioType = type;
            Id = id;
            Uri = uri;
            IdType = service;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((T)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)AudioType, Uri);
        }
        public virtual bool Equals(IAudioId other)
        {
            if (other is T genid)
            {
                return genid.Uri == Uri;
            }
            return false;
        }

        public AudioService IdType { get; set; }
        public AudioType AudioType { get; set; }
        public string Uri { get; set; }
        public string Id { get; set; }

        public abstract string ToMercuryUri(string locale);
        public abstract string ToHexId();
        public override string ToString() => Uri;
    }
}
