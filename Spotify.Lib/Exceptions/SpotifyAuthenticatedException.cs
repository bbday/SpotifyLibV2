using System;
using System.Diagnostics;

namespace Spotify.Lib.Exceptions
{
    public class SpotifyAuthenticatedException : Exception
    {
        public SpotifyAuthenticatedException(APLoginFailed loginFailed) :
            base(loginFailed.ErrorCode.ToString())
        {
            Debug.WriteLine(loginFailed.ErrorDescription);
            LoginFailed = loginFailed;
        }

        public APLoginFailed LoginFailed { get; }
    }
}