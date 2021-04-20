using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public delegate void PlayerStateChanged(MediaPlaybackState state, TrackOrEpisode? metadata);

    public interface ISpotifyPlayer : IDisposable
    {
        Task<Exception> IncomingStream(AbsChunkedStream stream);

        /// <summary>
        /// Gets invoked and you should Play/Resume the playback
        /// </summary>
        /// <param name="withSeek">With an initial seek. -1 if no seek</param>
        void Resume(int withSeek);

        /// <summary>
        /// Gets invoked and you should Pause the playback
        /// </summary>
        void Pause();

        /// <summary>
        /// Gets invoked and you should Seek.
        /// </summary>
        void Seek(object sender, int position);

        /// <summary>
        /// You should return a TimeSpan of the current mediaplayer's position.
        /// </summary>
        TimeSpan Position { get; }

        /// <summary>
        /// You MUST invoke methods on this delegate in order to update Spotify.
        /// </summary>
        PlayerStateChanged StateChanged { get; set; }

        Task<AbsChunkedStream> TryFetchStream(ISpotifyId id);

        void Inactive();

        void SetVolume(object sender, int value);

        int VolumeMax { get; }
        int VolumeSteps { get; }
        double NormalizedVolume { get; }
    }
}
