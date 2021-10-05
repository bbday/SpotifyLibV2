using System;
using Spotify;

namespace SpotifyLib
{
    public class SpotifyAuthenticationException : Exception
    {
        public SpotifyAuthenticationException(APLoginFailed failed) : base(failed.ErrorCode.ToString())
        {
            Failed = failed;
        }
        public APLoginFailed Failed { get; }
    }
}
