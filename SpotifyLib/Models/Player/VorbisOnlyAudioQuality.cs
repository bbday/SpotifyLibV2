using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Spotify.Metadata.Proto;

namespace SpotifyLib.Models.Player
{
    public static class VorbisOnlyAudioQuality
    {
        public static AudioFile GetVorbisFile(IEnumerable<AudioFile> files)
            => files.FirstOrDefault(a =>
                a.HasFormat && SuperAudioFormatHelper.Get(a.Format) == SuperAudioFormat.Vorbis);

        public static AudioFile GetFile(List<AudioFile> files,
            AudioQuality preferred)
        {
            var matches = AudioQualityHelper.GetMatches(files,
                preferred);
            var vorbisFile = VorbisOnlyAudioQuality.GetVorbisFile(matches);
            if (vorbisFile != null)
                return vorbisFile;
            vorbisFile = GetVorbisFile(files);
            Debug.WriteLine(vorbisFile != null
                ? $"Using {vorbisFile.Format} because preferred {preferred} could not be found."
                : $"Couldn't find any vorbis file. Available {string.Join(", ", files.Select(a => a.Format))}");

            return vorbisFile;
        }
    }

    public class SuperAudioFormatHelper
    {
        public static SuperAudioFormat Get(AudioFile.Types.Format format)
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
