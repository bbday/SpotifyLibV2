using System;
using System.IO;
using System.Threading.Tasks;

namespace SpotifyLibV2.Audio
{
    public interface IGeneralAudioStream : IDisposable
    {
        NormalizationData NormalizationData { get; set; }
        int DecryptTimeMs();
        Stream Stream();
    }
}