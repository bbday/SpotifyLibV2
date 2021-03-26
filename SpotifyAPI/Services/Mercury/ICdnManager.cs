using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Services.Mercury
{
    public interface ICdnManager
    {
        Task<Uri> GetAudioUrl(ByteString fileId);
        Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, int chunk);
        Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, long rangeStart, long rangeEnd);

        Task<IGeneralAudioStream> LoadTrack(TrackId id,
            IAudioQualityPicker audioQualityPicker,
            bool preload,
            [CanBeNull] IHaltListener haltListener);
    }
}
