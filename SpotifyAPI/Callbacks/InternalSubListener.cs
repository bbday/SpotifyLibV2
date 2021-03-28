using JetBrains.Annotations;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Callbacks
{
    internal class InternalSubListener
    {
        private readonly bool _isSub;
        private readonly ISubListener _listener;
        private readonly string _uri;

        internal InternalSubListener([NotNull] string uri,
            [NotNull] ISubListener listener,
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

        internal void Dispatch([NotNull] MercuryResponse resp)
        {
            _listener.OnEvent(resp);
        }
    }
}