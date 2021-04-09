using System;
using System.Linq;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Helpers.Extensions;

namespace SpotifyLibrary.Models.Ids
{
    public class StationId : StandardIdEquatable<StationId>
    {
        public StationId(string uri, JObject item) :
            base(uri, uri.Split(':').Last(), AudioType.Station, AudioService.Spotify)
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
