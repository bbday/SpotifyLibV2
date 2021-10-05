using System;
using System.Diagnostics;
using Spotify;

namespace SpotifyLib
{
    public class SpotifyConnectionException : Exception
    {
        internal SpotifyConnectionException(APResponseMessage responseMessage) :
            base(responseMessage.ToString())
        {
            Debug.WriteLine(responseMessage.ToString());
            LoginFailed = responseMessage;
        }

        public APResponseMessage LoginFailed { get; }
    }
}