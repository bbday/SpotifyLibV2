using JetBrains.Annotations;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Interfaces;

namespace SpotifyLibV2.Listeners
{
    public interface IEventsListener
    {
        void OnContextChanged([NotNull] string newUri);

        void OnTrackChanged([NotNull] IPlayableId id,
            [CanBeNull] IPlayableItem metadata);

        void OnPlaybackPaused(long trackTime);

        void OnPlaybackResumed(long trackTime);

        void OnTrackSeeked(long trackTime);

        void OnMetadataAvailable([NotNull] IPlayableItem metadata);

        void OnPlaybackHaltStateChanged(bool halted, long trackTime);

        void OnInactiveSession(bool timeout);

        void OnVolumeChanged(float volume);

        void OnPanicState();
    }
}