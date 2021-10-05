using System;
using System.Linq;
using Connectstate;
using Google.Protobuf;
using Spotify.Metadata.Proto;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models
{
    public class ImageId 
    {
        public ImageId(string uri)
        {
            Uri = uri;
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

                builder.Metadata[key] = new ImageId(image.FileId).Uri;
            }
        }
        public ImageId(ByteString hexByteString) : this($"spotify:image:{hexByteString.ToByteArray().BytesToHex()}")
        { }

        public string Uri { get; }
    }
}
