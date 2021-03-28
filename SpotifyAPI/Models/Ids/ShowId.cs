using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base62;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models.Enums;

namespace SpotifyLibrary.Models.Ids
{
        public class ShowId : StandardIdEquatable<ShowId>
        {
            public ShowId(string uri) : base(uri, uri.Split(':').Last(), 
                AudioType.Show, AudioService.Spotify)
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

