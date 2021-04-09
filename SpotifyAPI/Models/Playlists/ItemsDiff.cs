using System.Collections.Generic;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Playlists
{
    public readonly struct ItemsDiff : IDiffResult
    {
        public ItemsDiff(RevisionId fromRevision,
            RevisionId toRevision,
            bool hasChanged,
            IEnumerable<HermesPlaylistOperation> operations)
        {
            FromRevision = fromRevision;
            ToRevision = toRevision;
            HasChanged = hasChanged;
            Operations = operations;
            DiffType = DiffType.ItemChange;
        }

        public RevisionId FromRevision { get; }
        public RevisionId ToRevision { get; }
        public bool HasChanged { get; }
        public IEnumerable<HermesPlaylistOperation> Operations { get; }

        public DiffType DiffType { get; }
    }
}