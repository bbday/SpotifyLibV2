using System;
using System.Collections.Generic;
using System.Text;
using Spotify;
using SpotifyLibV2.Api;

namespace SpotifyLibV2
{
    public interface ISpotifySession
    {
        APWelcome ApWelcome { get; set; }
        APLoginFailed ApLoginFailed { get; set; }
        ISpotifyApiClient SpotifyApiClient { get; }
        ISpotifyReceiver SpotifyReceiver { get; }
        string CountryCode { get; }
    }
}
