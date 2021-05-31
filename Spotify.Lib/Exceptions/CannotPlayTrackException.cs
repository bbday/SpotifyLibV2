using System;

namespace Spotify.Lib.Exceptions
{
    public enum CannotPlayReason
    {
        NoFilesFound
    }
    public class CannotPlayTrackException : Exception
    {
        public CannotPlayTrackException(CannotPlayReason reason) : base($"Cannot play track. Reason: {reason}")
        {

        }
    }
}
