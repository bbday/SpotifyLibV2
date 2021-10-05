using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spotify.Metadata.Proto;

namespace SpotifyLib.Models.Player
{
    public enum AudioQuality
    {
        NORMAL,
        HIGH,
        VERY_HIGH
    }

    public static class AudioQualityHelper
    {
        public static AudioQuality GetQuality(AudioFile.Types.Format format)
        {
            switch (format)
            {
                case AudioFile.Types.Format.Mp396:
                case AudioFile.Types.Format.OggVorbis96:
                case AudioFile.Types.Format.Aac24Norm:
                    return AudioQuality.NORMAL;
                case AudioFile.Types.Format.Mp3160:
                case AudioFile.Types.Format.Mp3160Enc:
                case AudioFile.Types.Format.OggVorbis160:
                case AudioFile.Types.Format.Aac24:
                    return AudioQuality.HIGH;
                case AudioFile.Types.Format.Mp3320:
                case AudioFile.Types.Format.Mp3256:
                case AudioFile.Types.Format.OggVorbis320:
                case AudioFile.Types.Format.Aac48:
                    return AudioQuality.VERY_HIGH;
                default:
                    throw new ArgumentException("Unknown format: " + format);
            }
        }

        public static IEnumerable<AudioFile> GetMatches(List<AudioFile> files,
            AudioQuality quality) => files
            .Where(a => a.HasFormat && GetQuality(a.Format) == quality);
    }
}
