using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpotifyLibV2.Models.Response.ArtistMercuryInfo
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public class ArtstInfo
    {
        [J("artistGid")] public string ArtistGid { get; set; }
        [J("userCanEdit")] public bool UserCanEdit { get; set; }
        [J("name")] public string Name { get; set; }
        [J("mainImageUrl")] public Uri MainImageUrl { get; set; }
        [J("headerImage")] public Image HeaderImage { get; set; }
        [J("autobiography")] public Autobiography Autobiography { get; set; }
        [J("biography")] public string Biography { get; set; }
        [J("images")] public List<Image> Images { get; set; }
        [J("imagesCount")] public long ImagesCount { get; set; }
        [J("globalChartPosition")] public long GlobalChartPosition { get; set; }
        [J("monthlyListeners")] public long MonthlyListeners { get; set; }
        [J("monthlyListenersDelta")] public long MonthlyListenersDelta { get; set; }
        [J("followerCount")] public long FollowerCount { get; set; }
        [J("followingCount")] public long FollowingCount { get; set; }
        [J("playlists")] public Playlists Playlists { get; set; }
        [J("cities")] public List<City> Cities { get; set; }
    }
    public class City
    {
        [JsonIgnore] public string Listeners { get; set; }
        [JsonIgnore] public string Index { get; set; }
        [J("country")] public string Country { get; set; }
        [J("region")] public string Region { get; set; }
        [J("city")] public string CityCity { get; set; }
        [J("listeners")] public int list { get; set; }

    }


    public class Image
    {
        [J("id")] public string Id { get; set; }
        [J("moderationUri")] public string ModerationUri { get; set; }
        [J("uri")] public Uri Uri { get; set; }
        [J("width")] public long Width { get; set; }
        [J("height")] public long Height { get; set; }
    }

    public class Playlists
    {
        [J("entries")] public List<Entry> Entries { get; set; }
    }

    public class Entry
    {
        [J("uri")] public string Uri { get; set; }
        [J("name")] public string Name { get; set; }
        [J("imageUrl")] public Uri ImageUrl { get; set; }
        [J("owner")] public Owner Owner { get; set; }
        [J("listeners")] public long Listeners { get; set; }
    }

    public class Owner
    {
        [J("name")] public string Name { get; set; }
        [J("uri")] public string Uri { get; set; }
    }
}