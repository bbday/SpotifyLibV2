using System;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyProto;

namespace SpotifyLibrary.Services.Mercury
{
    public interface ICdnManager
    {
        Task<(Track Track, AudioFile File)> GetFile(ISpotifyId spotifyId,
            IAudioQualityPicker audioQualityPicker);
        Task<ByteString> GetAudioKey(Track track, AudioFile file);
        Task<Uri> GetAudioUrl(ByteString fileId);
        Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, int chunk);
        Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, long rangeStart, long rangeEnd);

        Task<IGeneralAudioStream> LoadTrack(TrackId id,
            IAudioQualityPicker audioQualityPicker,
            bool preload,
            [CanBeNull] IHaltListener haltListener);
    }
}
