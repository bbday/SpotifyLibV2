using System;
using System.Threading.Tasks;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Player
{
    public interface ISpotifyPlayer : IDisposable
    {
        /// <summary>
        /// The attached <see cref="SpotifyClient"/> session. This gets set automatically when attaching the player.
        /// </summary>
        SpotifyClient Session { get; set; }
        /// <summary>
        /// This method gets invoked when a new stream is available. This method gets called by <see cref="ICdnManager"/>
        /// You should use this method to preload the stream into your mediaplayer.
        /// </summary>
        /// <param name="stream">The stream to load</param>
        /// <returns></returns>
        Task StreamReady(IGeneralAudioStream stream);

        /// <summary>
        /// Gets invoked and you should Play/Resume the playback
        /// </summary>
        /// <param name="withSeek">With an initial seek</param>
        void Resume(int withSeek);

        /// <summary>
        /// Gets invoked and you should Pause the playback
        /// </summary>
        void Pause();

        /// <summary>
        /// New chunk is available for streaming. You could use this to cache a chunk.
        /// </summary>
        /// <param name="id">ID of the playing item</param>
        /// <param name="chunk">Encrypted Data of the incoming chunk</param>
        /// <param name="chunkIndex">Index out of total chunks</param>
        /// <param name="chunks">Total number of chunks</param>
        /// <param name="b">Boolean indicating whether or not the chunk was retrieved from cache.</param>
        void ChunkReceived(ISpotifyId id, byte[] chunk, int chunkIndex, int chunks, bool b);
        /// <summary>
        /// Gets invoked and you should Seek.
        /// </summary>
        void Seek(int position);

        /// <summary>
        /// You should return a TimeSpan of the current mediaplayer's position.
        /// </summary>
        TimeSpan Position { get; }

        /// <summary>
        /// You MUST invoke methods on this delegate in order to update Spotify.
        /// </summary>
        PlayerStateChanged StateChanged { get; set; }
    }
}
