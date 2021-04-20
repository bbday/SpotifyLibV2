using System;
using System.Diagnostics;
using System.Threading;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models
{
    public class KeyCallBack : IKeyCallBack, IDisposable
    {
        private static readonly long AudioKeyRequestTimeout = 5000;
        private byte[] _reference;
        private readonly ManualResetEvent _referenceLock = new ManualResetEvent(false);

        public void Key(byte[] key)
        {
            _reference = key;
            _referenceLock.Set();
            _referenceLock.Reset();
        }

        public void Error(short code)
        {
            Debug.WriteLine("Audio key error, code: {0}", code);
            _reference = null;
            _referenceLock.Set();
            _referenceLock.Reset();
        }

        public byte[]? WaitResponse()
        {
            _referenceLock.WaitOne(TimeSpan.FromMilliseconds(AudioKeyRequestTimeout));
            return _reference;
        }

        public void Dispose()
        {
            _referenceLock.Dispose();
        }
    }
}