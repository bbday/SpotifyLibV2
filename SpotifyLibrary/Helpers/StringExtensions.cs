using System.Text.RegularExpressions;
using SpotifyLibrary.Ids;

namespace Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }
        public static ISpotifyId? UriToIdConverter(this string input)
        {
            if (input.IsEmpty())
            {
                return null;
            }

            switch (input.Split(':')[1])
            {
                case "station":
                    //TODO
                    return null;
                case "track":
                    return new TrackId(input) as ISpotifyId;
                case "artist":
                    return new ArtistId(input) as ISpotifyId;
                case "album":
                    return new AlbumId(input) as ISpotifyId;
                case "show":
                    return new ShowId(input) as ISpotifyId;
                case "episode":
                    return new EpisodeId(input) as ISpotifyId;
                case "playlist":
                    return new PlaylistId(input) as ISpotifyId;
                case "collection":
                    return new LinkId(input) as ISpotifyId;
                case "app":
                    //Link
                    return new LinkId(input) as ISpotifyId;
                case "user":
                    //"spotify:user:7ucghdgquf6byqusqkliltwc2:collection
                    var regexMatch = Regex.Match(input, "spotify:user:(.*):playlist:(.{22})");
                    if (regexMatch.Success)
                    {
                        return new PlaylistId(input) as ISpotifyId;
                    }
                    else
                    {
                        regexMatch = Regex.Match(input, "spotify:user:(.*):collection");
                        if (regexMatch.Success)
                        {
                            return new LinkId(input) as ISpotifyId;
                        }

                        return new UserId(input) as ISpotifyId;
                    }
                default:
                    return null;
            }
        }
    }
}
