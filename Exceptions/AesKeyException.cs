using System;

namespace SpotifyLibV2.Exceptions
{
    public class AesKeyException : Exception
    {
        public AesKeyException(string message) : base(message)
        {

        }
    }
}
