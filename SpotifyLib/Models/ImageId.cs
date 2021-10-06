using System;
using System.Collections.Generic;
using System.Linq;
using Connectstate;
using Google.Protobuf;
using Spotify.Metadata.Proto;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models
{
    public class ImageId 
    {
        public ImageId(string id)
        {
            Uri = $"spotify:image:{id.ToLowerInvariant()}";
            Id = id.ToLowerInvariant();
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
        public static IEnumerable<(Image.Types.Size Size, string Uri)> GetImages(
            ImageGroup group)
        {
            return @group.Image.Select(image => (image.Size, new ImageId(image.FileId).Id));
        }
        public ImageId(ByteString hexByteString) : 
            this(hexByteString.ToByteArray().BytesToHex().ToLowerInvariant())
        { }

        public string Id { get; }
        public string Uri { get; }
    }
}
