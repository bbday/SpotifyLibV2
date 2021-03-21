using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using SpotifyLibV2.Helpers.Extensions;

namespace SpotifyLibV2.Audio
{
    public class BinaryReader2 : BinaryReader
    {
        public BinaryReader2(System.IO.Stream stream) : base(stream) { }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public Int16 ReadInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public Int64 ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }
        public float ReadSingle()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }
        public UInt32 ReadUInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

    }
    public class NormalizationData
    {
        public readonly float track_gain_db;
        public readonly float track_peak;
        public readonly float album_gain_db;
        public readonly float album_peak;

        private NormalizationData(float track_gain_db, float track_peak, float album_gain_db, float album_peak)
        {
            this.track_gain_db = track_gain_db;
            this.track_peak = track_peak;
            this.album_gain_db = album_gain_db;
            this.album_peak = album_peak;

            Debug.WriteLine("Loaded normalization data, track_gain: {0}, track_peak: {1}, album_gain: {}, album_peak: {2}",
                track_gain_db, track_peak, album_gain_db, album_peak);
        }

        public static NormalizationData Read([NotNull] Stream input)
        {
            using var @in = new MemoryStream();
            input.CopyTo(@in);
            var currentPos = @in.Position;
            if (@in.SkipBytes(144) != 144) throw new IOException();

            byte[] data = new byte[4 * 4];
            @in.ReadComplete(data, 0, data.Length);
            @in.Position = currentPos;


            var b = @in.ToArray();
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(b);
            using var b2 = new BinaryReader2(@in);
            return new NormalizationData(b2.ReadSingle(),
                b2.ReadSingle(), b2.ReadSingle(), b2.ReadSingle());
        }

        public float GetFactor(float normalisationPregain)
        {
            float normalisationFactor = (float)Math.Pow(10, (track_gain_db + normalisationPregain) / 20);
            if (normalisationFactor * track_peak > 1)
            {
                Debug.WriteLine(
                    "Reducing normalisation factor to prevent clipping. Please add negative pregain to avoid.");
                normalisationFactor = 1 / track_peak;
            }

            return normalisationFactor;
        }
    }
}