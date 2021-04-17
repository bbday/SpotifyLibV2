using System;
using System.Threading;
using Nito.AsyncEx;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Models
{
    internal class SyncCallback : ICallback, IDisposable
    {
        private readonly AsyncAutoResetEvent _waitHandle = new AsyncAutoResetEvent(false);
        private MercuryResponse _reference;
        private CancellationToken ct;

        public SyncCallback(CancellationToken ct)
        {
            this.ct = ct;
        }

        void ICallback.Response(MercuryResponse response)
        {
            _reference = response;
            _waitHandle.Set();
        }

        internal MercuryResponse WaitResponse()
        {
            _waitHandle.Wait(ct);
            return _reference;
        }

        public void Dispose()
        {
            //_waitHandle.
            _reference.Dispose();
        }
    }
}