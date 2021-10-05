using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Response
{
    public readonly struct MercuryArtistInsight
    {
        [JsonConstructor]
        public MercuryArtistInsight(string name, string mainImageUrl, UriImage headerImage,
            InsightsAutoBiographyObject? autobiography, string? biography, UriImage[] images,
            short imagesCount, short globalChartPosition, ulong monthlyListeners, long monthlyListenersDelta,
            ulong followerCount,
            ulong followingCount,  InsightCity[] cities, InsightsPlaylistEntriesObject? playlists)
        {
            Name = name;
            MainImageUrl = mainImageUrl;
            HeaderImage = headerImage;
            Autobiography = autobiography;
            Biography = biography;
            Images = images;
            ImagesCount = imagesCount;
            GlobalChartPosition = globalChartPosition;
            MonthlyListeners = monthlyListeners;
            MonthlyListenersDelta = monthlyListenersDelta;
            FollowerCount = followerCount;
            FollowingCount = followingCount;
            Cities = cities;
            Playlists = playlists;
            HasNoImages = imagesCount == 0;
        }

        public string Name { get; }
        public string MainImageUrl { get; }
        public UriImage HeaderImage { get; }
        public InsightsAutoBiographyObject? Autobiography { get; }
        public string? Biography { get; }
        public UriImage[] Images { get; }
        public short ImagesCount { get; }
        public short GlobalChartPosition { get; }
        public ulong MonthlyListeners { get; }
        public long MonthlyListenersDelta { get; }
        public ulong FollowerCount { get; }
        public ulong FollowingCount { get; }
        public InsightsPlaylistEntriesObject? Playlists { get; }
        public InsightCity[] Cities { get; }

        public bool HasNoImages { get; }

        public IEnumerable<IndexedCity> AmountOfCities(int cities, int skip)
        {
            return Cities?.Skip(skip)?.Take(cities)
                ?.Select((z,i)=> new IndexedCity((ushort)(skip + i + 1), z));
        }
    }
    public readonly struct IndexedCity
    {
        public IndexedCity(ushort index, InsightCity city)
        {
            Index = index;
            City = city;
            Title = $"{city.City}, {city.Country}";
        }

        public ushort Index { get; }
        public InsightCity City { get; }
        public string Title { get; }
    }

    public readonly struct InsightsAutoBiographyObject
    {
        [JsonConstructor]
        public InsightsAutoBiographyObject(string body, Dictionary<string, string> links)
        {
            Body = body;
            Links = links;
        }

        public string Body { get; }
        public Dictionary<string, string> Links { get; }
    }
    public readonly struct InsightsPlaylistEntriesObject
    {
        [JsonConstructor]
        public InsightsPlaylistEntriesObject(IEnumerable<ArtistPlaylistEntry> entries)
        {
            Entries = entries;
        }

        public IEnumerable<ArtistPlaylistEntry> Entries { get; }
    }

    public readonly struct ArtistPlaylistEntry : ISpotifyItem
    {
        [JsonConstructor]
        public ArtistPlaylistEntry(SpotifyId uri, string name, string imageUrl, Quick owner)
        {
            Uri = uri;
            Name = name;
            ImageUrl = imageUrl;
            Owner = owner;
        }

        [JsonConverter(typeof(UriToSpotifyIdConverter))]
        public SpotifyId Uri { get; }
        public string Name { get; }
        public string ImageUrl { get; }
        public Quick Owner { get; }
    }

    public readonly struct InsightCity
    {
        [JsonConstructor]
        public InsightCity(string country, string region, string city, ulong listeners)
        {
            Country = country;
            Region = region;
            City = city;
            Listeners = listeners;
        }

        public string Country { get; }
        public string Region{ get; }
        public string City { get; }
        public ulong Listeners { get; }
    }
}
