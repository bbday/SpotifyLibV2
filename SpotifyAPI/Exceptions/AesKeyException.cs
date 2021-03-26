using System;

namespace SpotifyLibrary.Exceptions
{
    public class AesKeyException : Exception
    {
        public AesKeyException(string message) : base(message)
        {

        }
    }
}
