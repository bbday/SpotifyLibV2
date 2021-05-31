using System;

namespace Spotify.Lib.Exceptions
{
    public class UnsupportedContextException : Exception
    {
        public UnsupportedContextException(string message) : base(message)
        {

        }
        public static UnsupportedContextException CannotPlayAnything()
        {
            return new("Nothing from this context can or should be played!");
        }
    }
}
