using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using SpotifyProto;

namespace SpotifyLibV2.Audio.Qualities
{
    public class SuperAudioFormatHelper
    {
        public static SuperAudioFormat Get([NotNull] AudioFile.Types.Format format)
        {
            switch (format)
            {
                case AudioFile.Types.Format.OggVorbis96:
                case AudioFile.Types.Format.OggVorbis160:
                case AudioFile.Types.Format.OggVorbis320:
                    return SuperAudioFormat.Vorbis;
                case AudioFile.Types.Format.Mp3256:
                case AudioFile.Types.Format.Mp3320:
                case AudioFile.Types.Format.Mp3160:
                case AudioFile.Types.Format.Mp396:
                case AudioFile.Types.Format.Mp3160Enc:
                    return SuperAudioFormat.Mp3;
                case AudioFile.Types.Format.Aac24:
                case AudioFile.Types.Format.Aac48:
                case AudioFile.Types.Format.Aac24Norm:
                    return SuperAudioFormat.Aac;
                default:
                    return SuperAudioFormat.Unknown;
            }
        }
    }

    public enum SuperAudioFormat
    {
        Unknown,
        Mp3,
        Vorbis,
        Aac
    }
}
