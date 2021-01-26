using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace SpotifyLibV2.Models.Response
{
    public class DefaultUser : GenericSpotifyItem
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonPropertyName(("name"))]
        public string Name { get; set; }

        /// <summary>
        /// profile image url https cdn.
        /// </summary>
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Number of followers the user has
        /// </summary>
        [JsonPropertyName("followers_count")]
        public int FollowersCount { get; set; }

        /// <summary>
        /// Number of followers the user has
        /// </summary>
        [JsonPropertyName("following_count")]
        public int FollowingCount { get; set; }

        /// <summary>
        /// Number of followers the user has
        /// </summary>
        [JsonPropertyName("is_following")]
        public bool IsFollowing { get; set; }

        /// <summary>
        /// Boolean whether or not user follows current user. Can be null (if artist)
        /// </summary>
        [JsonPropertyName("is_followed")]
        public bool? IsFollowed { get; set; }
    }
    public class UserProfileResponse : DefaultUser
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonPropertyName("total_public_playlists_count")]
        public int PlaylistsCount { get; set; }

        /// <summary>
        /// Boolean whether or not the fetched user is current user.
        /// </summary>
        [JsonPropertyName("is_current_user")]
        public bool IsCurrentUser { get; set; }

        /// <summary>
        /// Boolean whether or not the user has a name.
        /// </summary>
        [JsonPropertyName("has_spotify_name")]
        public bool HasName { get; set; }

        /// <summary>
        /// Boolean whether or not the user has a image.
        /// </summary>
        [JsonPropertyName("has_spotify_image")]
        public bool HasImage { get; set; }

        /// <summary>
        /// Not sure about this one!
        /// </summary>
        [JsonPropertyName("color")]
        public long Color { get; set; }

        /// <summary>
        /// Recently played Artists
        /// </summary>
        [JsonPropertyName("recently_played_artists")]
        public IEnumerable<RecentlyPlayedArtist> RecentlyPlayedArtists { get; set; }

        /// <summary>
        /// Recently played Artists
        /// </summary>
        [JsonPropertyName("public_playlists")]
        public IEnumerable<PublicPlaylist> PublicPlaylists { get; set; }
    }

    public class RecentlyPlayedArtist : GenericSpotifyItem
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonPropertyName(("name"))]
        public string Name { get; set; }

        /// <summary>
        /// profile image url https cdn.
        /// </summary>
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Number of followers the user has
        /// </summary>
        [JsonPropertyName("followers_count")]
        public int FollowersCount { get; set; }

        /// <summary>
        /// Number of followers the user has
        /// </summary>
        [JsonPropertyName("is_following")]
        public bool IsFollowing { get; set; }
    }

    public class PublicPlaylist : GenericSpotifyItem
    {
        /// <summary>
        /// Name of the playlist
        /// </summary>
        [JsonPropertyName(("name"))]
        public string Name { get; set; }

        /// <summary>
        /// image url https cdn.
        /// </summary>
        [JsonPropertyName("image_url")]
        [CanBeNull]
        public Uri ImageUrl { get; set; }

        /// <summary>
        /// Number of followers the playlist has
        /// </summary>
        [JsonPropertyName("followers_count")]
        public int FollowersCount { get; set; }

        /// <summary>
        /// Name of the owner of the playlist
        /// </summary>
        [JsonPropertyName("owner_name")]
        public string OwernName { get; set; }

        /// <summary>
        /// Spotify uri of the owner of the playlist
        /// </summary>
        [JsonPropertyName("owner_uri")]
        public string OwnerUri { get; set; }

        /// <summary>
        /// bool indication if the user is following the playlist.
        /// </summary>
        [JsonPropertyName("is_following")]
        public bool IsFollowing { get; set; }
    }
}