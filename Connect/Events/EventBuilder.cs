using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using SpotifyLibV2.Connect.Interfaces;

namespace SpotifyLibV2.Connect.Events
{
    public readonly struct EventBuilder : IDisposable
    {
        private readonly MemoryStream _body;
        public EventBuilder([NotNull] Enumeration type)
        {
            _body = new MemoryStream(256);
            AppendNoDelimiter(type.Id.ToString());
            Append(type.Unknown.ToString());
        }

        public EventBuilder Append(string str)
        {
            _body.WriteByte(0x09);
            AppendNoDelimiter(str);
            return this;
        }

        public EventBuilder Append(char c)
        {
            var j = (byte)c;
            _body.WriteByte(0x09);
            _body.WriteByte(j);
            return this;
        }

        public byte[] ToArray()
        {
            return _body.ToArray();
        }
        private void AppendNoDelimiter([CanBeNull] string str)
        {
            str ??= "";
            var bytesToWrite = Encoding.UTF8.GetBytes(str);
            _body.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        public static string ToString([NotNull] byte[] body)
        {
            var result = new StringBuilder();
            foreach (var b in body)
                if (b == 0x09) result.Append('|');
                else result.Append((char)b);

            return result.ToString();
        }

        public void Dispose()
        {
            _body?.Dispose();
        }
    }
}
