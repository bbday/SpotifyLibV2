using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static bool IsHexString(this string input, int length)
        {
            //TODO
            return false;
        }

        [CanBeNull]
        public static ISpotifyId UriToIdConverter(this string input)
        {
            if (input == null)
            {
                return null;
            }

            switch (input.Split(':')[1])
            {
                case "station":
                    //TODO
                    return null;
                case "track":
                    return new TrackId(input);
                case "artist":
                    return new ArtistId(input);
                case "album":
                    return new AlbumId(input);
                case "show":
                    //TODO
                    return null;
                case "episode":
                    return new EpisodeId(input);
                case "playlist":
                    return new PlaylistId(input);
                case "collection":
                    return new LinkId(input);
                case "user":
                    //"spotify:user:7ucghdgquf6byqusqkliltwc2:collection
                    var regexMatch = Regex.Match(input, "spotify:user:(.*):playlist:(.{22})");
                    if (regexMatch.Success)
                    {
                        return new PlaylistId(input);
                    }
                    else
                    {
                        regexMatch = Regex.Match(input, "spotify:user:(.*):collection");
                        if (regexMatch.Success)
                        {
                            return new LinkId(input);
                        }

                        return new UserId(input);
                    }
                default:
                    return null;
            }
        }
    }
}