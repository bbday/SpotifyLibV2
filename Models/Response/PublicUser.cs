using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyLibV2.Models.Response
{
    public class Followers
    {

    }
    public class PublicUser
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; } = default!;

        public Dictionary<string, string> ExternalUrls { get; set; } = default!;

        public Followers Followers { get; set; } = default!;

        public string Href { get; set; } = default!;

        public string Id { get; set; } = default!;

        public List<UserImage> Images { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string Uri { get; set; } = default!;
    }

    public class UserImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}

