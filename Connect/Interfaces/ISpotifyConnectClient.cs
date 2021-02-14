using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibV2.Models.Public;
using SpotifyLibV2.Models.Request;
using SpotifyLibV2.Models.Request.PlaybackRequests;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyConnectClient
    {
        Task<bool> Connect();

        Task<PlayingChangedRequest?> FetchCurrentlyPlaying();

        Task<bool> RequestPlay(IPlaybackRequest request);
        Task<bool> TransferDevice(string deviceId);
    }
}
