using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibrary.Exceptions
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
