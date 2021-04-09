using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Playlists
{
    public interface IDiffResult
    {
        RevisionId FromRevision { get; }
        RevisionId ToRevision { get; }
        bool HasChanged { get; }
        DiffType DiffType { get; }
    }
}
