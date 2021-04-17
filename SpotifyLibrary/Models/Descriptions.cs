using MediaLibrary.Interfaces;

namespace SpotifyLibrary.Models
{
    public class Descriptions
    {
        public Descriptions(string name, IAudioId id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public IAudioId Id { get; }
    }
}
