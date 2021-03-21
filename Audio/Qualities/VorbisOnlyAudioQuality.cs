using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using SpotifyProto;

namespace SpotifyLibV2.Audio.Qualities
{
    public class VorbisOnlyAudioQuality : IAudioQualityPicker
    {
        private readonly AudioQualityHelper.AudioQuality _preferred;

        public VorbisOnlyAudioQuality([NotNull] AudioQualityHelper.AudioQuality preferred)
        {
            this._preferred = preferred;
        }

        public static AudioFile GetVorbis([NotNull] List<AudioFile> files)
        {
            return files.FirstOrDefault(z => z.HasFormat &&
                                             SuperAudioFormatHelper.Get(z.Format) == SuperAudioFormat.Vorbis);
        }

        public AudioFile GetFile(List<AudioFile> files)
        {
            var matches = _preferred.GetMatches(files);
            var vorbis = GetVorbis(matches);
            if (vorbis == null)
            {
                vorbis = GetVorbis(files);
                if (vorbis != null)
                    Debug.WriteLine("Using {0} because preferred {1} couldn't be found.", vorbis.Format, _preferred);
                else
                    Debug.WriteLine("Couldn't find any Vorbis file, available: {0}", files.Count);
            }

            return vorbis;
        }

    }
}
