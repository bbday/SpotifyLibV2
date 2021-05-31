﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Ids
{
    public class LinkId : ISpotifyId
    {
        public LinkId(string uri, string sectionId)

        {
            Uri = uri;
            AudioType = AudioItemType.Link;
            LinkType = LinkType.Genre;
            GenreType = sectionId;
        }

        public LinkId(string uri)

        {
            Uri = uri;
            AudioType = AudioItemType.Link;

            var regexMatches = new (Regex, LinkType CollectionTracks)[]
            {
                (new Regex("spotify:collection:tracks"), LinkType.CollectionTracks),
                (new Regex("spotify:user:(.*):collection"), LinkType.CollectionTracks),
                (new Regex("spotify:genre:(.*)"), LinkType.Genre),
                (new Regex("spotify:app:genre:(.*)"), LinkType.Genre),
                (new Regex("spotify:app:hub:browse:internal:(.*)"), LinkType.Puff),
                (new Regex("spotify:daily-mix-hub"), LinkType.DailyMixHub),
                (new Regex(""), LinkType.Unknown)
            };
            var firstMatch = regexMatches.FirstOrDefault(z
                => z.Item1.Match(uri ?? "").Success);
            if (firstMatch.Item1 != null)
            {
                LinkType = firstMatch.CollectionTracks;
                switch (LinkType)
                {
                    case LinkType.Genre:
                        GenreType = uri.Split(':').Last();
                        break;
                    case LinkType.Puff:
                        GenreType = uri.Split(':').Last()
                            .Replace("-page", "");
                        break;
                    case LinkType.DailyMixHub:
                        GenreType = "made-for-x-hub";
                        break;
                }
            }
        }

        public bool IsGenre => LinkType == LinkType.Genre;
        public string? GenreType { get; }

        public LinkType LinkType { get; }
        public AudioItemType AudioType { get; }
        public string Uri { get; }
        public string Id => throw new NotImplementedException();

        public string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public string ToHexId()
        {
            throw new NotImplementedException();
        }

        public bool Equals(ISpotifyId other)
        {
            if (other is LinkId link)
                return link.Uri == Uri;
            return false;
        }
    }
}