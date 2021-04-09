using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibrary.Models.Response
{
    public class LocationsResponse
    {
        public List<LocationItem> Results { get; set; }
    }
    public class LocationItem
    {
        public string Location { get; set; }
        public string Geohash { get; set; }
        public long GeonameId { get; set; }
    }
}
