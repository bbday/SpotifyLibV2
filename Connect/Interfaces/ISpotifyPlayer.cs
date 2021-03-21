using System;
using System.IO;
using System.Threading.Tasks;
using SpotifyLibV2.Audio;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Interfaces;
using SpotifyProto;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyPlayer
    {
        ISpotifySession Session { get; set; }
        Task PlayItem(IPlayableId id);
        void ChunkReceived(byte[] data, int index, bool encrypted);
    }
}
