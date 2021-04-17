using System;
using System.IO;

namespace Extensions
{
    public static class StreamExtensions
    {

        private static readonly int MAX_SKIP_BUFFER_SIZE = 2048;

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
        public static int SkipBytes(this MemoryStream input, int n)
        {
            int total = 0;
            int cur = 0;

            while ((total < n) && ((cur = (int)input.Skip(n - total)) > 0))
            {
                total += cur;
            }

            return total;
        }

        public static long Skip(this Stream input, long n)
        {
            long remaining = n;
            int nr;

            if (n <= 0)
            {
                return 0;
            }

            int size = (int)Math.Min(MAX_SKIP_BUFFER_SIZE, remaining);
            byte[] skipBuffer = new byte[size];
            while (remaining > 0)
            {
                nr = input.Read(skipBuffer, 0, (int)Math.Min(size, remaining));
                if (nr < 0)
                {
                    break;
                }

                remaining -= nr;
            }
            return n - remaining;
        }
        public static void WriteText(this Stream stream, string text)
        {
            if (!stream.CanWrite)
                throw new Exception("Stream is not writeable.");

            using (var writer = new StreamWriter(stream))
            {
                writer.Write(text);
                writer.Flush();
            }
        }

        public static string ReadText(this Stream stream)
        {
            if (!stream.CanRead)
                throw new Exception("Stream is not readable.");

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
