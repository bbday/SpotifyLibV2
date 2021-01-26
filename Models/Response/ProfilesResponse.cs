using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyLibV2.Models.Response
{
    public class ProfilesResponse
    {
        [JsonPropertyName("profiles")] 
        public IEnumerable<DefaultUser> Profiles { get; set; }
    }
}
