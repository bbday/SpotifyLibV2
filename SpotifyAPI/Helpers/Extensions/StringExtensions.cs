using System.Text.RegularExpressions;
using JetBrains.Annotations;
using SpotifyLibrary.Models.Ids;
using SpotifyLibV2.Helpers;

namespace SpotifyLibrary.Helpers.Extensions
{
    public static class StringExtensions
    {

        public static string SizedString(this long size)
        {
            if (size < 1024) return size.ToString() + " B";
            else size /= 1024;
            long rest;
            if (size < 1024) return size.ToString() + " KB";
            else
            {
                rest = size % 1024;
                size /= 1024;
            }
            if (size < 1024)
            {
                size *= 100;
                return (size / 100).ToString() + "." + ((rest * 100 / 1024 % 100)).ToString() + " MB";
            }
            else
            {
                size = size * 100 / 1024;
                return ((size / 100)).ToString() + "." + ((size % 100)).ToString() + " GB";
            }
        }
        public static bool IsHexString(this string input, int length)
        {
            //TODO
            return false;
        }

        [CanBeNull]
        public static ISpotifyId UriToIdConverter(this string input)
        {
            if (input == null || input.IsEmpty())
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
                    return new ShowId(input);
                case "episode":
                    return new EpisodeId(input);
                case "playlist":
                    return new PlaylistId(input);
                case "collection":
                    return new LinkId(input);
                case "app":
                    //Link
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