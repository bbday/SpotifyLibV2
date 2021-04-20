using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Audio
{
    public interface IPlayerSessionListener
    {
        void FinishedLoading(TrackOrEpisode metadata);
    }
}
