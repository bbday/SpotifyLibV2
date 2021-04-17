using System;
using System.Linq;
using Extensions;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json.Linq;

namespace SpotifyLibrary.Ids
{
    public class StationId : StandardIdEquatable<StationId>
    {
        public StationId(string uri, JObject item) :
            base(uri, uri.Split(':').Last(), AudioItemType.Station, AudioServiceType.Spotify)
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

        public IAudioId NavigateToId { get; }
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
