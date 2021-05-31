using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models
{
    public readonly struct Descriptions
    {
        public Descriptions(string name, ISpotifyId id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public ISpotifyId Id { get; }
    }
}