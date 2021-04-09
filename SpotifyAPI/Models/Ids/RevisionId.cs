using System;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Ids
{
    public struct RevisionId : IEquatable<RevisionId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64Revision">Example: AZoRRAAAAAAXBuxcRWgStFlarg/wqSZz</param>
        public RevisionId(string snapshot_id)
        {
            OriginalBase64 = snapshot_id;
            //AZoRRAAAAAAXBuxcRWgStFlarg/wqSZz
            var bytes = Convert.FromBase64String(snapshot_id);
            var hex = BitConverter.ToString(bytes);
            var hexDumped = hex.Replace("-", "").ToLower();

            //AZoRRAAAAAAXBuxcRWgStFlarg/wqSZz
            //019a1144 | 000000001706ec5c456812b4595aae0ff0a92673

            //AAAAMcZWwzJm1e0bRgipKej9OmJxOfkT
            //00000031 | c656c33266d5ed1b4608a929e8fd3a627139f913

            var decimalPart = hexDumped.Substring(0, 8);
            var idPart = hexDumped.Substring(8, hexDumped.Length - 8);
            Number = int.Parse(decimalPart,
                System.Globalization.NumberStyles.HexNumber);
            Id = idPart;
        }
        [JsonProperty("snapshot_id")]
        public string OriginalBase64 { get; }
        public int Number { get; set; }
        public string Id { get; set; }

        public override string ToString() => $"{Number},{Id}";

        public bool Equals(RevisionId other)
        {
            return Number == other.Number && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is RevisionId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Number, Id);
        }
    }
}
