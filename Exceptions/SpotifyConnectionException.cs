using System;
using System.Diagnostics;

namespace SpotifyLibV2.Exceptions
{
    public class SpotifyConnectionException : Exception
    {
        public SpotifyConnectionException(string message) : base(message)
        {
            Debug.WriteLine(message);
        }
    }
}