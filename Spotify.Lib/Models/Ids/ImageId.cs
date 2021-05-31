using System;
using System.Linq;
using System.Text;
using Base62;
using Connectstate;
using Google.Protobuf;
using Spotify.Lib.Helpers;
using SpotifyProto;

namespace Spotify.Lib.Models.Ids
{
    public class ImageId : StandardIdEquatable<ImageId>
    {
        public ImageId(string uri) : base(uri, uri.Split(':').Last(), AudioItemType.Image)
        {
        }
        public static void PutAsMetadata(ProvidedTrack builder,
            ImageGroup group)
        {
            foreach (var image in group.Image)
            {
                string key;
                switch (image.Size)
                {
                    case Image.Types.Size.Default:
                        key = "image_url";
                        break;
                    case Image.Types.Size.Small:
                        key = "image_small_url";
                        break;
                    case Image.Types.Size.Large:
                        key = "image_large_url";
                        break;
                    case Image.Types.Size.Xlarge:
                        key = "image_xlarge_url";
                        break;
                    default:
                        continue;
                }

                var uri = ImageId.FromHex(image.FileId.ToByteArray().BytesToHex()).Uri;
                builder.Metadata[key] = uri;
            }
        }
        public static ImageId FromHex(string hex)
        {
            var j = "spotify:image:" + hex.ToLowerInvariant();
            return new ImageId(j);
        }

        public override string ToMercuryUri(string locale)
        {
            throw new NotImplementedException();
        }

        public override string ToHexId()
        {
            throw new NotImplementedException();
        }
    }
}