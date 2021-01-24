using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace SpotifyLibV2.Models.Response.ArtistMercuryInfo
{
    public class ArtistInsights
    {
        [J("name")] public string Name { get; set; }
        [J("artistUri")] public string ArtistUri { get; set; }
        [J("isVerified")] public bool IsVerified { get; set; }
        [J("biography")] public string Biography { get; set; }
        [J("autobiography")] public Autobiography Autobiography { get; set; }
        [J("gallery")] public Gallery Gallery { get; set; }
        [J("avatar")] public Avatar Avatar { get; set; }
        [J("monthlyListeners")] public long MonthlyListeners { get; set; }
        [J("globalChartPosition")] public long GlobalChartPosition { get; set; }
        [J("image2Migration")] public long Image2Migration { get; set; }
    }

    public class Autobiography
    {
        [J("body")] public string Body { get; set; }
        [J("urls")] public List<object> Urls { get; set; }
        [J("links")] public Links Links { get; set; }
    }

    public class Links
    {
        [J("wikipedia", NullValueHandling = NullValueHandling.Ignore)] public Uri Wikipedia { get; set; }
        [J("twitter", NullValueHandling = NullValueHandling.Ignore)] public Uri Twitter { get; set; }
        [J("instagram", NullValueHandling = NullValueHandling.Ignore)] public Uri Instagram { get; set; }
        [J("facebook", NullValueHandling = NullValueHandling.Ignore)] public Uri Facebook { get; set; }
    }

    public class Avatar
    {
        [J("size")] public string Size { get; set; }
        [J("uri")] public Uri Uri { get; set; }
        [J("width")] public long Width { get; set; }
        [J("height")] public long Height { get; set; }
    }

    public class Gallery
    {
        [J("total")] public long Total { get; set; }
        [J("images")] public List<InsightImage> Images { get; set; }
        [J("gallerySource")] public string GallerySource { get; set; }
    }

    public partial class InsightImage
    {
        [J("originalId")] public string OriginalId { get; set; }
        [J("image")] public Avatar ImageImage { get; set; }
    }
}