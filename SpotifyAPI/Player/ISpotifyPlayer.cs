using System;
using System.Threading.Tasks;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Player
{
    public interface ISpotifyPlayer : IDisposable
    {
        SpotifyClient Session { get; set; }
        Task StreamReady(IGeneralAudioStream stream);
        void Resume(bool fromStart, int withSeek);
        void Pause();
        void ChunkReceived(ISpotifyId id, byte[] chunk, int chunkIndex, int chunks, bool b);
        void Seek(int position);
        TimeSpan Position { get; }
        PlayerStateChanged StateChanged { get; set; }
    }
}
