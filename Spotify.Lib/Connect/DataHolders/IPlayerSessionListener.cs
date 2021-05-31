using Spotify.Lib.Models;

namespace Spotify.Lib.Connect.DataHolders
{
    internal interface IPlayerSessionListener
    {
        void FinishedLoading(PlayerSessionHolder session, TrackOrEpisode metadata);
    }
}