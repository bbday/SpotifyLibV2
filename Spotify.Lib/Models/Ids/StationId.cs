using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Helpers;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Ids
{
    public class StationId : StandardIdEquatable<StationId>
    {
        public StationId(string uri, JObject item) :
            base(uri, uri.Split(':').Last(), AudioItemType.Station)
        {
            var navigateToUri = item["events"]["click"]["data"]["uri"].ToString();
            var parseType = navigateToUri.UriToIdConverter();
            NavigateToId = parseType switch
            {
                ArtistId artistId => new ArtistId(navigateToUri),
                AlbumId albumId => new AlbumId(navigateToUri),
                PlaylistId playlistId => new PlaylistId(navigateToUri),
                TrackId trackId => new TrackId(navigateToUri),
                _ => throw new ArgumentOutOfRangeException(nameof(parseType))
            };
        }

        public ISpotifyId NavigateToId { get; }

        public override string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public override string ToHexId()
        {
            throw new NotImplementedException();
        }
    }
}