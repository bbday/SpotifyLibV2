using System;
using System.Threading;
using Nito.AsyncEx;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models
{
    internal class SyncCallback : ICallback, IDisposable
    {
        private readonly AsyncAutoResetEvent _waitHandle = new(false);
        private MercuryResponse _reference;
        private readonly CancellationToken ct;

        public SyncCallback(CancellationToken ct)
        {
            this.ct = ct;
        }

        void ICallback.Response(MercuryResponse response)
        {
            _reference = response;
            _waitHandle.Set();
        }

        public void Dispose()
        {
            //_waitHandle.
            //_reference.Dispose();
        }

        internal MercuryResponse WaitResponse()
        {
            _waitHandle.Wait(ct);
            return _reference;
        }
    }
}