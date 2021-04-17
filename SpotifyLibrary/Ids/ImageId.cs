using System;
using System.Linq;
using Connectstate;
using Google.Protobuf;
using MediaLibrary.Enums;
using SpotifyLibrary.Helpers;
using SpotifyProto;

namespace SpotifyLibrary.Ids
{
    public class ImageId : StandardIdEquatable<ImageId>
    {
        public ImageId(string uri) : base(uri, uri.Split(':').Last(), AudioItemType.Image, AudioServiceType.Spotify)
        {
        }
        public static void PutAsMetadata(ProvidedTrack builder,
            ImageGroup group)
        {
            foreach (var image in group.Image)
            {
                String key;
                switch (image.Size)
                {
                    case global::SpotifyProto.Image.Types.Size.Default:
                        key = "image_url";
                        break;
                    case global::SpotifyProto.Image.Types.Size.Small:
                        key = "image_small_url";
                        break;
                    case global::SpotifyProto.Image.Types.Size.Large:
                        key = "image_large_url";
                        break;
                    case global::SpotifyProto.Image.Types.Size.Xlarge:
                        key = "image_xlarge_url";
                        break;
                    default:
                        continue;
                }

                builder.Metadata[key] = new ImageId(image.FileId).Uri;
            }
        }
        public ImageId(ByteString hexByteString) : this($"spotify:image:{hexByteString.ToByteArray().BytesToHex()}")
        { }

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
