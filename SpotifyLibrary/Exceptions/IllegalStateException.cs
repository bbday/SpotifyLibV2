using System;

namespace SpotifyLibrary.Exceptions
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