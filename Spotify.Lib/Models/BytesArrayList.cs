using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Utilities;
using Spotify.Lib.Helpers;

namespace Spotify.Lib.Models
{
    internal class InternalStream : MemoryStream
    {
        private List<byte[]> _elementData;
        private int offset = 0;
        private int sub;

        public InternalStream(List<byte[]> elementData)
        {
            _elementData = elementData;
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        protected override void Dispose(bool disposing)
        {
            _elementData = null;
            base.Dispose(disposing);
        }


        public override int Read(byte[] buffer,
            int offset,
            int count)
        {
            if (offset < 0 || count < 0 || count > buffer.Length - offset)
                throw new IndexOutOfRangeException();
            if (count == 0) return 0;

            if (sub >= _elementData.Count)
                return -1;

            var i = 0;
            while (true)
            {
                var copy = Math.Min(count - i, _elementData[sub].Length - offset);
                Array.Copy(_elementData[sub], offset,
                    buffer,
                    offset + i,
                    copy);
                i += copy;
                offset += copy;

                if (i == count)
                    return i;

                if (offset >= _elementData[sub].Length)
                {
                    offset = 0;
                    if (++sub >= _elementData.Count)
                        return i == 0 ? -1 : i;
                }
            }
        }
    }

    public class BytesArrayList : IEnumerable<byte[]>, IDisposable
    {
        private readonly List<byte[]> _elementData;

        public BytesArrayList()
        {
            _elementData = new List<byte[]>();
        }

        private BytesArrayList(byte[][] buffer)
        {
            _elementData = buffer.ToList();
        }

        private int _size => _elementData.Count;

        public void Dispose()
        {
            Stream().Dispose();
            _elementData.Clear();
            Dispose(true);
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            return _elementData.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string ReadIntoString(int index)
        {
            return Encoding.Default.GetString(_elementData[0]);
        }

        public BytesArrayList CopyOfRange(int from, int to)
        {
            return new(
                _elementData.Skip(from)
                    .Take(to - from)
                    .ToArray());
        }

        public void Add(byte[] e)
        {
            _elementData.Add(e);
        }

        public Stream Stream(string[] payloads)
        {
            var bytes = new byte[payloads.Length][];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = Encoding.Default.GetBytes(payloads[i]);
            return new BytesArrayList(bytes).Stream();
        }

        public string ToHex()
        {
            var array = new string[_size];
            var copy = _elementData.ToArray();
            for (var i = 0; i < copy.Length; i++) array[i] = copy[i].BytesToHex();
            return Arrays.ToString(array);
        }

        public Stream Stream()
        {
            return new InternalStream(_elementData);
        }

        public virtual void Dispose(bool dispo)
        {
        }
    }
}