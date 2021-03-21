using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibV2.Connect.Events;
using SpotifyLibV2.Ids;
using SpotifyProto;

namespace SpotifyLibV2.Audio.Cdn
{
    public interface ICdnManager
    {
        Task<Uri> GetAudioUrl(ByteString fileId);
        Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, int chunk);
        Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, long rangeStart, long rangeEnd);

        Task<Stream> LoadTrack(TrackId id,
            IAudioQualityPicker audioQualityPicker,
            bool preload,
            [CanBeNull] IHaltListener haltListener);
    }
}
