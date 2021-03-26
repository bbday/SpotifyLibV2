using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
    public class LinkId : ISpotifyId
    {
        public LinkId(string uri) 
        
        {
            Uri = uri;
            IdType = AudioService.Spotify;
            AudioType = AudioType.Link;

            var regexMatches = new (Regex, LinkType CollectionTracks)[]
            {
                (new Regex($"spotify:collection:tracks"), LinkType.CollectionTracks),
                (new Regex("spotify:user:(.*):collection"), LinkType.CollectionTracks)
            };
            var firstMatch = regexMatches.FirstOrDefault(z 
                => z.Item1.Match(uri).Success);
            if (firstMatch.Item1 != null)
            {
                LinkType = firstMatch.CollectionTracks;
            }
        }


        public bool Equals(IAudioId other)
        {
            if (other is LinkId link)
                return link.Uri == Uri;
            return false;
        }

        public LinkType LinkType { get; }
        public AudioService IdType { get; }
        public AudioType AudioType { get; }
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
    }
}
