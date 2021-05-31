using System;

namespace Spotify.Lib.Exceptions
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException(string message) : base(message)
        {

        }

        public IllegalStateException()
        {

        }
    }
}