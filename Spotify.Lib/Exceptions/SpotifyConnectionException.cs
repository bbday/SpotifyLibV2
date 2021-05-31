using System;
using System.Diagnostics;

namespace Spotify.Lib.Exceptions
{
    public class SpotifyConnectionException : Exception
    {
        public SpotifyConnectionException(APResponseMessage responseMessage) :
            base(responseMessage.ToString())
        {
            Debug.WriteLine(responseMessage.ToString());
            LoginFailed = responseMessage;
        }

        public APResponseMessage LoginFailed { get; }
    }
}