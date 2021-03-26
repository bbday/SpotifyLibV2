using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using SpotifyProto;

namespace SpotifyLibrary.Connect.Qualities
{
    public static class AudioQualityHelper
    {
        public enum AudioQuality
        {
            UNKNOWN,
            NORMAL,
            HIGH,
            VERY_HIGH
        }

        private static AudioQuality GetAudioQuality([NotNull] AudioFile.Types.Format format)
        {
            switch (format)
            {
                case AudioFile.Types.Format.Mp396:
                case AudioFile.Types.Format.OggVorbis96:
                    return AudioQuality.NORMAL;
                case AudioFile.Types.Format.Mp3160:
                case AudioFile.Types.Format.Mp3160Enc:
                case AudioFile.Types.Format.OggVorbis160:
                    break;
                case AudioFile.Types.Format.Aac24:
                    return AudioQuality.HIGH;
                case AudioFile.Types.Format.OggVorbis320:
                case AudioFile.Types.Format.Mp3256:
                case AudioFile.Types.Format.Mp3320:
                    return AudioQuality.VERY_HIGH;
                default:
                    return AudioQuality.UNKNOWN;
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }

            return AudioQuality.NORMAL;
        }

        public static List<AudioFile> GetMatches(
            this AudioQuality preffered,
            [NotNull] List<AudioFile> files)
            => files.Where(file => file.HasFormat && GetAudioQuality(file.Format) == preffered).ToList();
    }
}
