using System.Collections.Generic;

namespace SpotifyLibV2.Listeners
{
    public static class ListenersHolder
    {
        public static List<ISpotifySessionListener>
            SpotifySessionConcurrentDictionary =
                new List<ISpotifySessionListener>();

    }
}
