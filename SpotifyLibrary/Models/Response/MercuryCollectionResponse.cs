using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;

namespace SpotifyLibrary.Models.Response
{
    public class MercuryCollectionResponse<T> where T : ICollectionItem
    {
        [JsonPropertyName("item")]
        public IEnumerable<T> Items { get; set; }
    }
    public abstract class AbsMercuryCollectionItem : ICollectionItem
    {
        private IAudioId? _itemId;
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
        [JsonPropertyName("added_at")]
        public long AddedAtTimeStamp { get; set; }

        public DateTime AddedAtDate => UnixTimeStampToDateTime(AddedAtTimeStamp);

        public IAudioId ItemId
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
                        _itemId =  Ids.TrackId.FromHex(hexData);
                        break;
                    case AudioItemType.Artist:
                        _itemId = Ids.ArtistId.FromHex(hexData);
                        break;
                }

                return _itemId;
            }
        }

        public bool Equals(IAudioId other)
        {
            return ItemId.Equals(other);
        }

        protected bool Equals(AbsMercuryCollectionItem other)
        {
            return ItemId.Equals(other.ItemId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AbsMercuryCollectionItem)obj);
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

        public abstract AudioItemType ItemType { get; }
    }

    public class MercuryTrackCollectionItem : AbsMercuryCollectionItem
    {
        public override AudioItemType ItemType => AudioItemType.Track;
    }
    public class MercuryArtistCollectionItem : AbsMercuryCollectionItem
    {
        public override AudioItemType ItemType => AudioItemType.Artist;
    }
}
