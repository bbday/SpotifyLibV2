using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Models.Response.Mercury
{
    public interface ICollectionItem : IEquatable<ISpotifyId>
    {
        public long AddedAtTimeStamp { get; set; }
        public ISpotifyId? ItemId { get; }
        public DateTime AddedAtDate { get; }
    }
    public class MercuryCollectionResponse<T> where T : ICollectionItem
    {
        [JsonPropertyName("item")]
        public IEnumerable<T> Items { get; set; }
    }
    public class MercuryCollectionItem : ICollectionItem
    {
        private ISpotifyId? _itemId;
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
        [JsonPropertyName("added_at")]
        public long AddedAtTimeStamp { get; set; }

        public DateTime AddedAtDate => UnixTimeStampToDateTime(AddedAtTimeStamp);

        public ISpotifyId? ItemId
        {
            get
            {
                if (_itemId != null) return _itemId;
                var bytes = Convert.FromBase64String(Identifier);
                var hex = BitConverter.ToString(bytes);
                var hexData = hex.Replace("-", "").ToLower();
                switch (ItemType)
                {
                    case AudioItemType.Track:
                        _itemId = Ids.TrackId.FromHex(hexData);
                        break;
                    case AudioItemType.Album:
                        _itemId = Ids.AlbumId.FromHex(hexData);
                        break;
                }

                return _itemId;
            }
        }

        public bool Equals(ISpotifyId other)
        {
            return ItemId.Equals(other);
        }

        protected bool Equals(MercuryCollectionItem other)
        {
            return ItemId.Equals(other.ItemId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MercuryCollectionItem)obj);
        }

        public override int GetHashCode()
        {
            return (ItemId != null ? ItemId.GetHashCode() : 0);
        }
        public static DateTime UnixTimeStampToDateTime(
            long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("Type")]
        public AudioItemType ItemType { get; set; }
    }
}
