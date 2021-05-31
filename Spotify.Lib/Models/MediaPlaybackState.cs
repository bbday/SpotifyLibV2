namespace Spotify.Lib.Models
{
    public enum MediaPlaybackState
    {
        None,
        Buffering,
        FinishedLoading,
        TrackStarted,
        TrackPaused,
        TrackPlayed,
        NewTrack,
        PositionChanged,
        VolumeChanged
    }
}