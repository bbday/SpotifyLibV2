using System;

namespace SpotifyLib
{
    public class AesKeyException : Exception
    {
        public AesKeyException(string message) : base(message)
        {

        }
    }
}
