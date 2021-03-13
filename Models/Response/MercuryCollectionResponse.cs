using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Models.Response
{
    public interface ICollectionItem : IEquatable<IAudioId>
    {
        public long AddedAtTimeStamp { get; set; }
        public IPlayableId TrackId { get; }
        public DateTime AddedAtDate { get; }
    }
    public class MercuryCollectionResponse
    {
        [JsonPropertyName("item")]
        public IEnumerable<MercuryCollectionItem> Items { get; set; }
    }
    public class MercuryCollectionItem : ICollectionItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
        [JsonPropertyName("added_at")]
        public long AddedAtTimeStamp { get; set; }

        public DateTime AddedAtDate => AddedAtTimeStamp.UnixTimeStampToDateTime();

        public IPlayableId TrackId
        {
            get
            {
                var bytes = Convert.FromBase64String(Identifier);
                var hex = BitConverter.ToString(bytes);
                var hexData = hex.Replace("-", "").ToLower();
                return Ids.TrackId.FromHex(hexData);
            }
        }

        public bool Equals(IAudioId other)
        {
            return TrackId.Equals(other);
        }

        protected bool Equals(MercuryCollectionItem other)
        {
            return TrackId.Equals(other.TrackId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MercuryCollectionItem) obj);
        }

        public override int GetHashCode()
        {
            return (TrackId != null ? TrackId.GetHashCode() : 0);
        }
    }
}
