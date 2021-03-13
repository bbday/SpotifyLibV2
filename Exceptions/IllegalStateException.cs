using System;

namespace SpotifyLibV2.Exceptions
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
