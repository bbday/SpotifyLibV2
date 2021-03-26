using System;
using System.IO;
using Google.Protobuf;
using SpotifyLibrary.Models.Ids;
using SpotifyProto;

namespace SpotifyLibrary.Audio
{
    public interface IGeneralAudioStream : IDisposable
    {
        NormalizationData NormalizationData { get; set; }
        int DecryptTimeMs();
        Stream Stream();
        ByteString FileId { get; }
        ISpotifyId Id { get; }
        Episode Episode { get; }
        Track Track { get; }
    }
}
