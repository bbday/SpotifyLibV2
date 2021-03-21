using System.Collections.Generic;
using JetBrains.Annotations;
using SpotifyProto;

namespace SpotifyLibV2.Audio
{
    public interface IAudioQualityPicker
    {
        AudioFile GetFile([NotNull] List<AudioFile> files);
    }
}
