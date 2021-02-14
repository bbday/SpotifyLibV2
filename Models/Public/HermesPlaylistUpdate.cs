using System;
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
        public HermesPlaylistOperation(Op op)
        {
            IEnumerable<Item> items = null;
            switch (op.Kind)
            {
                case Op.Types.Kind.Unknown:
                    Operation = PlaylistOperation.Unknown;
                    break;
                case Op.Types.Kind.Add:
                    Operation = PlaylistOperation.Add;
                    items = op.Add.Items;
                    FromIndex = op.Add.FromIndex;
                    break;
                case Op.Types.Kind.Rem:
                    Operation = PlaylistOperation.Remove;
                    items = op.Rem.Items;
                    FromIndex = op.Rem.FromIndex;
                    break;
                case Op.Types.Kind.Mov:
                    Operation = PlaylistOperation.Move;
                    FromIndex = op.Mov.FromIndex;
                    ToIndex = op.Mov.ToIndex;
                    break;
                case Op.Types.Kind.UpdateItemAttributes:
                    break;
                case Op.Types.Kind.UpdateListAttributes:
                    //TODO!
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Items = items?.Select(z =>
            {
                var uri = z.Uri;
                var type = uri.Split(':')[1];
                return type switch
                {
                    "track" => (new TrackId(uri) as ISpotifyId, z.Attributes),
                    "episode" => (new EpisodeId(uri) as ISpotifyId, z.Attributes),
                    _ => (new UnknownId(uri) as ISpotifyId, z.Attributes)
                };
            });
        }

        [CanBeNull] public int? ToIndex { get; }
        public int FromIndex { get; }
        [CanBeNull] public IEnumerable<(ISpotifyId Id, ItemAttributes attributes)> Items { get; }
        public PlaylistOperation Operation { get; }
    }
}
