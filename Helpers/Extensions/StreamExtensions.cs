using System.IO;

namespace SpotifyLibV2.Helpers.Extensions
{
    public static class StreamExtensions
    {
        public static void ReadComplete(this Stream stream, byte[] buffer, int offset, int count)
        {
            int num = 0;
            while (num < count)
                num += stream.Read(buffer, offset + num, count - num);
        }

        public static void Write(this MemoryStream input,
            string text)
        {
            var b = System.Text.Encoding.Default.GetBytes(text);
            input.Write(b, 0, b.Length);
        }
        public static int GetShort(this MemoryStream input) => GetShort(input.ToArray(), (int)input.Position, true);
        private static short GetShort(byte[] obj0, int obj1, bool obj2)
        {
            return (short)(!obj2 ? (int)GetShortL(obj0, obj1) : (int)GetShortB(obj0, obj1));
        }

        private static short GetShortB(byte[] obj0, int obj1) => MakeShort(obj0[obj1], obj0[obj1 + 1]);

        private static short GetShortL(byte[] obj0, int obj1)
        {
            return MakeShort(obj0[obj1 + 1], obj0[obj1]);
        }

        private static short MakeShort(byte obj0, byte obj1) =>
            (short)((int)(sbyte)obj0 << 8 | (int)(sbyte)obj1 & (int)byte.MaxValue);

    }
}
