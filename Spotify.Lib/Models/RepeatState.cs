using Spotify.Lib.Attributes;

namespace Spotify.Lib.Models
{
    public enum RepeatState
    {
        [String("track")] Track,

        [String("context")] Context,

        [String("off")] Off
    }
}