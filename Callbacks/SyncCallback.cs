using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Callbacks
{
    internal class SyncCallback : ICallback
    {
        private readonly System.Threading.EventWaitHandle _waitHandle = new System.Threading.AutoResetEvent(false);
        private MercuryResponse _reference;

        internal MercuryResponse WaitResponse()
        {
            _waitHandle.WaitOne();
            return _reference;
        }

        void ICallback.Response(MercuryResponse response)
        {
            _reference = response;
            _waitHandle.Set();
        }
    }
}