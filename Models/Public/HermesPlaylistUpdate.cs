using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Spotify.Playlist4.Proto;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Models.Public
{
    public enum PlaylistOperation
    {
        Unknown,
        Add,
        Remove,
        Move
    }
    public class HermesPlaylistUpdate
    {
        internal HermesPlaylistUpdate(
            PlaylistId playlistUri,
            IReadOnlyList<Op> ops)
        {

            Playlist = playlistUri;
            Ops = ops.Select(z => new HermesPlaylistOperation(z)).ToImmutableList();
        }
        public PlaylistId Playlist { get; }
        public IReadOnlyList<HermesPlaylistOperation> Ops { get; }
    }

    public class HermesPlaylistOperation
    {
        internal HermesPlaylistOperation(Op op)
        {
            IEnumerable<Item> items = null;
            if (op.Add != null)
            {
                Operation = PlaylistOperation.Add;
                items = op.Add.Items;
                FromIndex = op.Add.FromIndex;
            }
            else if (op.Mov != null)
            {
                Operation = PlaylistOperation.Move;
                FromIndex = op.Mov.FromIndex;
                ToIndex = op.Mov.ToIndex;
            }
            else if (op.Rem != null)
            {
                Operation = PlaylistOperation.Remove;
                items = op.Rem.Items;
                FromIndex = op.Rem.FromIndex;
            }
            else
            {
                Operation = PlaylistOperation.Unknown;
            }

            Items = items?.Select(z =>
            {
                var uri = z.Uri;
                var type = uri.Split(':')[1];
                return type switch
                {
                    "track" => new TrackId(uri) as ISpotifyId,
                    "episode" => new EpisodeId(uri) as ISpotifyId,
                    _ => new UnknownId(uri) as ISpotifyId
                };
            });
        }

        [CanBeNull] public int? ToIndex { get; }
        public int FromIndex { get; }
        [CanBeNull] public IEnumerable<ISpotifyId> Items { get; }
        public PlaylistOperation Operation { get; }
    }
}
