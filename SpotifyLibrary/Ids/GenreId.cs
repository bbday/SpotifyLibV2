using MediaLibrary.Enums;
using MediaLibrary.Interfaces;

namespace SpotifyLibrary.Ids
{
    public class GenreId : IAudioId
    {
        public GenreId(string mercuryUri)
        {
            MercuryUri = mercuryUri;
        }
        public string MercuryUri { get; }
        public bool Equals(IAudioId other)
        {
            if (other is GenreId genreId)
                return genreId.MercuryUri == MercuryUri;
            return false;
        }

        public AudioServiceType IdType => AudioServiceType.Spotify;
        public AudioItemType AudioType => AudioItemType.Link;
    }
}
