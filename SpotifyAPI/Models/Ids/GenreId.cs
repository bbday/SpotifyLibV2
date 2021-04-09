using MusicLibrary.Enum;
using MusicLibrary.Interfaces;

namespace SpotifyLibrary.Models.Ids
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

        public AudioService IdType => AudioService.Spotify;
        public AudioType AudioType => AudioType.Link;
    }
}
