    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SpotifyLib.Enums;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models
{
    public enum PlayType
    {
        Spotify,
        Local
    }
    public readonly struct  SpotifyId : IEquatable<SpotifyId>, IComparable<SpotifyId>
    {
        public SpotifyId WithType(AudioItemType type) => new SpotifyId(
            $"spotify:{type.ToString().ToLower()}:{Id}");

        [JsonConstructor]
        public SpotifyId(string uri)
        {
            Uri = uri;
            var s =
                uri.SplitLines();
            if (s.MoveNext())
            {
                Source = (PlayType)Enum.Parse(typeof(PlayType), s.Current.Line.ToString(), true);
                if (s.MoveNext())
                {
                    Type = GetType(s.Current.Line, uri);
                    if (s.MoveNext())
                    {
                        Id = s.Current.Line.ToString();
                        return;
                    }
                }
            }

            throw new NotFiniteNumberException();
        }
        public PlayType Source { get; }
        public string Uri { get; }
        public AudioItemType Type { get; }
        public string Id { get; }
        public bool Equals(SpotifyId other)
        {
            return Uri == other.Uri;
        }

        public override bool Equals(object obj)
        {
            return obj is SpotifyId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Uri != null ? Uri.GetHashCode() : 0);
        }

        private static AudioItemType GetType(ReadOnlySpan<char> r,
           string uri)
        {
            switch (r)
            {
                case var start_group when
                    start_group.SequenceEqual("start-group".AsSpan()):
                    return AudioItemType.StartGroup;
                case var end_group when
                    end_group.SequenceEqual("end-group".AsSpan()):
                    return AudioItemType.EndGroup;
                case var end_group when
                    end_group.SequenceEqual("station".AsSpan()):
                    return AudioItemType.Station;
                case var track when
                    track.SequenceEqual("track".AsSpan()):
                    return AudioItemType.Track;
                case var artist when
                    artist.SequenceEqual("artist".AsSpan()):
                    return AudioItemType.Artist;
                case var album when
                    album.SequenceEqual("album".AsSpan()):
                    return AudioItemType.Album;
                case var show when
                    show.SequenceEqual("show".AsSpan()):
                    return AudioItemType.Show;
                case var episode when
                    episode.SequenceEqual("episode".AsSpan()):
                    return AudioItemType.Episode;
                case var playlist when
                    playlist.SequenceEqual("playlist".AsSpan()):
                    return AudioItemType.Playlist;
                case var collection when
                    collection.SequenceEqual("collection".AsSpan()):
                    return AudioItemType.Link;
                case var app when
                    app.SequenceEqual("app".AsSpan()):
                    return AudioItemType.Link;
                case var dailymixhub when
                    dailymixhub.SequenceEqual("daily-mix-hub".AsSpan()):
                    return AudioItemType.Link;
                case var user when
                    user.SequenceEqual("daily-mix-hub".AsSpan()):
                    {
                        var regexMatch = Regex.Match(uri, "spotify:user:(.*):playlist:(.{22})");
                        if (regexMatch.Success)
                        {
                            return AudioItemType.Playlist;
                        }

                        regexMatch = Regex.Match(uri, "spotify:user:(.*):collection");
                        return regexMatch.Success ? AudioItemType.Link : AudioItemType.User;
                    }
                default:
                    return AudioItemType.Link;
            }
        }

        public int Compare(SpotifyId x, SpotifyId y)
        {
            return string.Compare(x.Uri, y.Uri, StringComparison.Ordinal);
        }

        public int CompareTo(SpotifyId other)
        {
            return string.Compare(Uri, other.Uri, StringComparison.Ordinal);
        }
    }
}
