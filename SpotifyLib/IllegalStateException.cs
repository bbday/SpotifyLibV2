using System;

namespace SpotifyLib
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