using System;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Ids
{
    public class GenreId : ISpotifyId
    {
        public GenreId(string mercuryUri)
        {
            MercuryUri = mercuryUri;
        }

        public string MercuryUri { get; }
        public AudioItemType AudioType => AudioItemType.Link;
        public string Uri { get; }
        public string Id { get; }

        public string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public string ToHexId()
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISpotifyId other)
        {
            if (other is GenreId genreId)
                return genreId.MercuryUri == MercuryUri;
            return false;
        }
    }
}