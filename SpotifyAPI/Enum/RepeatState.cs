using SpotifyLibrary.Attributes;

namespace SpotifyLibrary.Enum
{
    public enum RepeatState
    {
        [String("track")] Track,

        [String("context")] Context,

        [String("off")] Off
    }
}
