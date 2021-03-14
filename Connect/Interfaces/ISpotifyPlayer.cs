using System;
using System.Threading.Tasks;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Ids;
using SpotifyProto;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyPlayer
    {
        TimeSpan Position { get; set; }
        MediaPlaybackState PlaybackState { get; set; }
        event EventHandler<object> PlaybackStateChanged;
        event EventHandler<object> MediaOpened;
        event EventHandler<object> MediaEnded;
        Task Pause();
        Task Pause(bool v);
        Task Resume();
        Task Play();
        Task<(Track track, Episode episode)> Load(IPlayableId id, bool preloaded,  int initialSeek);
        void NotActive();
        bool IsActive { get; set; }
        int Time { get; }

        Task ClearOutput();
    }
}
