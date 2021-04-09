using System.Threading;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Callbacks
{
    internal class SyncCallback : ICallback
    {
        private readonly EventWaitHandle _waitHandle = new AutoResetEvent(false);
        private MercuryResponse _reference;

        void ICallback.Response(MercuryResponse response)
        {
            _reference = response;
            _waitHandle.Set();
        }

        internal MercuryResponse WaitResponse()
        {
            _waitHandle.WaitOne();
            return _reference;
        }
    }
}