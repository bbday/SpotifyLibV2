using System;
using System.Linq;
using Base62;
using MediaLibrary.Enums;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Ids
{
        public class ShowId : StandardIdEquatable<ShowId>
        {
            public ShowId(string uri) : base(uri, uri.Split(':').Last(), 
                AudioItemType.Show, AudioServiceType.Spotify)
            {
            }
            public static ShowId FromHex(string hex)
            {
                var k = (Utils.HexToBytes(hex)).ToBase62(true);
                var j = "spotify:show:" + k;
                return new ShowId(j);
            }

            public override string ToMercuryUri(string locale)
                => throw new NotImplementedException();


            public override string ToHexId()
            {
                var decoded = Id.FromBase62(true);
                var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
                if (hex.Length > 32)
                {
                    hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
                }
                return hex;
            }
        }
    }

