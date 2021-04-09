using MusicLibrary.Interfaces;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Playlists
{
    public readonly struct NewListDiff : IDiffResult
    {
        public NewListDiff(
            RevisionId fromRevision,
            RevisionId toRevision,
            IAudioId playlist)
        {
            FromRevision = fromRevision;
            ToRevision = toRevision;
            HasChanged = true;
            DiffType = DiffType.ItemChange;
            Playlist = playlist;
        }
        public IAudioId Playlist { get; }
        public RevisionId FromRevision { get; }
        public RevisionId ToRevision { get; }
        public bool HasChanged { get; }
        public DiffType DiffType { get; }
    }
}