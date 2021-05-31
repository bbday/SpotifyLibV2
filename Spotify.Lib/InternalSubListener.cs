using Spotify.Lib.Interfaces;
using Spotify.Lib.Models;

namespace Spotify.Lib
{
    internal class InternalSubListener
    {
        private readonly bool _isSub;
        private readonly ISubListener _listener;
        private readonly string _uri;

        internal InternalSubListener(string uri,
            ISubListener listener,
            bool isSub)
        {
            _uri = uri;
            _listener = listener;
            _isSub = isSub;
        }

        internal bool Matches(string uri)
        {
            return uri.StartsWith(_uri);
        }

        internal void Dispatch(MercuryResponse resp)
        {
            _listener.OnEvent(resp);
        }
    }
}