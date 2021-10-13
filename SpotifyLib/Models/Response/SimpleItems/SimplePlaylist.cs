using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response.SimpleItems
{
    public struct SimplePlaylist : ISpotifyItem
    {
        [JsonConstructor]
        public SimplePlaylist(string name, List<UrlImage> images, bool collaborative, PublicUser owner, string description,
            SpotifyId uri, string snapshotId, FollowerObject followers)
        {
            Name = name;
            Images = images;
            Collaborative = collaborative;
            Owner = owner;
            Description = description;
            Uri = uri;
            SnapshotId = snapshotId;
            Followers = followers;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get;  }
        public List<UrlImage> Images { get;  }
        public bool Collaborative { get;  }
        public PublicUser Owner { get; }
        public FollowerObject Followers { get; }
        public string Description { get;  }
        [JsonPropertyName("snapshot_id")]
        public string SnapshotId { get; }
    }

    public readonly struct FollowerObject
    {
        [JsonConstructor]
        public FollowerObject(int total)
        {
            Total = total;
        }

        public int Total { get; }
    }
}
