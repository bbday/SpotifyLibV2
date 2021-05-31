using System;

namespace Spotify.Lib.Exceptions
{
    public class AesKeyException : Exception
    {
        public AesKeyException(string message) : base(message)
        {

        }
    }
}
