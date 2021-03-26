using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using SpotifyProto;

namespace SpotifyLibrary.Audio
{
    public interface IAudioQualityPicker
    {
        AudioFile GetFile([NotNull] List<AudioFile> files);
    }
}
