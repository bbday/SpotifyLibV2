using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SpotifyLibV2.Enums;

namespace SpotifyLibV2.Helpers
{
    public static class UriToAudioTypeConverter
    {
        public static AudioType GetAudioType(this string input)
        {
            var type = AudioType.Unknown;
            if (input == null)
            {
                return AudioType.Link;
            }
            switch (input.Split(':')[1])
            {
                case "station":
                    type = AudioType.Station;
                    break;
                case "track":
                    type = AudioType.Track;
                    break;
                case "artist":
                    type = AudioType.Artist;
                    break;
                case "album":
                    type = AudioType.Album;
                    break;
                case "show":
                    type = AudioType.Show;
                    break;
                case "episode":
                    type = AudioType.Episode;
                    break;
                case "playlist":
                    type = AudioType.Playlist;
                    break;
                case "collection":
                    type = AudioType.Link;
                    break;
                case "user":

                    //"spotify:user:7ucghdgquf6byqusqkliltwc2:collection
                    var regexMatch = Regex.Match(input, "spotify:user:(.*):playlist:(.{22})");
                    if (regexMatch.Success)
                    {
                        type = AudioType.Playlist;
                    }
                    else
                    {
                        regexMatch = Regex.Match(input, "spotify:user:(.*):collection");
                        if (regexMatch.Success)
                        {
                            type = AudioType.Link;
                            break;
                        }
                        type = AudioType.Profile;
                    }
                    break;
            }
            return type;
        }
    }
}
