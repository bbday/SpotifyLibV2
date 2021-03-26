using Newtonsoft.Json;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class PublicUser
    {
        [JsonProperty("id")]
        public string _id { get; set; }
        public string Uri { get; set; }
        [JsonIgnore]
        public IAudioId Id { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        public AudioType Type => AudioType.User;
    }
}
