using System;
using System.Collections.Generic;
using System.Text;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response
{
    public class ConcertsResponse
    {
        public ConcertArtist Artist { get; set; }
        [JsonProperty("userLocation")]
        public string UserLocation { get; set; }
        public List<ConcertItem> Concerts { get; set; }
    }
    public class ConcertArtist : ISpotifyItem
    {
        private List<UrlImage> _images;
        private IAudioId _id;
        private TrackId _topTrackId;
        public AudioService AudioService => AudioService.Spotify;
        public IAudioId Id => _id ??= new ArtistId(Uri);
        public AudioType Type => AudioType.Artist;

        public List<UrlImage> Images => _images ??= new List<UrlImage>
        {
            new UrlImage
            {
                Uri = ImageUri
            }
        };

        public TrackId TopTrackId => _topTrackId ??= new TrackId(TopTrackUri);
        public string TopTrackUri { get; set; }
        public string Name { get; set; }
        public string Description => Bio;
        public string Uri { get; set; }
        public string ImageUri { get; set; }
        public string Bio { get; set; }
    }
    public class ConcertItem
    {
        public ConcertObject Concert { get; set; }
        
        public bool NearUser { get; set; }
    }
    public class ConcertObject
    {
        public string Id { get; set; }
        public List<string> ArtistUris { get; set; }
        public List<ConcertArtist> Artists { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string Venue { get; set; }
        [JsonProperty("lat")]
        public float Latitude { get; set; }
        [JsonProperty("lon")]
        public float Lonitude { get; set; }
        public List<PartnerConcertObject> PartnerConcerts { get; set; }
        public DateObject Startdate { get; set; }
        public bool Festival { get; set; }
        public string ArtistNameTitle { get; set; }
        public string CarouselImage { get; set; }
        public bool IsParent { get; set; }
        public string Category { get; set; }
    }
    public struct DateObject
    {
        public DateTime Date { get; set; }
        public double UtcOffset { get; set; }
        public DateTime LocalDate { get; set; }
        public bool UnknownTime { get; set; }
    }
    public class PartnerConcertObject
    {
        public string PartnerId { get; set; }
        public string ConcertId { get; set; }
    }
}
