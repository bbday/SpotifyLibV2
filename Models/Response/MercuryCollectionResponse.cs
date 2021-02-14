using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Models.Response
{
    public class MercuryCollectionResponse
    {
        [JsonPropertyName("item")]
        public IEnumerable<MercuryCollectionItem> Items { get; set; }
    }
    public class MercuryCollectionItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
        [JsonPropertyName("added_at")]
        public long AddedAtTimeStamp { get; set; }

        public DateTime AddedAtDate => AddedAtTimeStamp.UnixTimeStampToDateTime();

        public TrackId TrackId
        {
            get
            {
                var bytes = Convert.FromBase64String(Identifier);
                var hex = BitConverter.ToString(bytes);
                var hexData = hex.Replace("-", "").ToLower();
                return TrackId.FromHex(hexData);
            }
        }
    }
}
