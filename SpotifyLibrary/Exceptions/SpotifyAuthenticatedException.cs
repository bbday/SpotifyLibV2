using System;
using System.Diagnostics;
using Spotify;

namespace SpotifyLibrary.Exceptions
{
    public class SpotifyAuthenticatedException : Exception
    {
        public SpotifyAuthenticatedException(APLoginFailed loginFailed) : base(loginFailed.ErrorCode.ToString())
        {
            Debug.WriteLine(loginFailed.ErrorDescription);
        }
    }
}