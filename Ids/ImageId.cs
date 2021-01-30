using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Base62;
using Connectstate;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyProto;

namespace SpotifyLibV2.Ids
{
    public class ImageId : ISpotifyId
    {
        public static void PutAsMetadata([NotNull] ProvidedTrack builder,
            [NotNull] ImageGroup group)
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
        public ImageId(ByteString hexByteString)
        {
            var hexId = hexByteString.ToByteArray().BytesToHex();
            var uri = $"spotify:image:{hexId}";
            Type = AudioType.Image;
            var regexMatch = Regex.Match(uri, "spotify:image:(.{40})");
            if (regexMatch.Success)
            {
                this.Id = regexMatch.Groups[1].Value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(uri), "Not a Spotify Image ID: " + uri);
            }
            this.Uri = uri;
        }

        public ImageId(string uri)
        {
            Type = AudioType.Image;
            var regexMatch = Regex.Match(uri, "spotify:image:(.{40})");
            if (regexMatch.Success)
            {
                this.Id = regexMatch.Groups[1].Value;
            }
            else
            {
                //cdn url.
            }
            this.Uri = uri;
        }

        public static ImageId BiggestImage(ImageGroup group)
        {
            global::SpotifyProto.Image biggest = null;
            foreach (var image in group.Image)
            {
                if (biggest == null || biggest.Size < image.Size)
                    biggest = image;
            }

            return biggest == null ? null : new ImageId(biggest.FileId);
        }

        public string Uri { get; }
        public string Id { get; }
        public string ToHexId()
        {
            var decoded = Id.FromBase62(true);
            var hex = BitConverter.ToString(decoded).Replace("-", string.Empty);
            if (hex.Length > 32)
            {
                hex = hex.Substring(hex.Length - 32, hex.Length - (hex.Length - 32));
            }
            return hex;
        }

        public string ToMercuryUri() => "hm://metadata/4/album/" + ToHexId();

        public AudioType Type { get; }

        protected bool Equals(ImageId other)
        {
            return Uri == other.Uri
                   && Id == other.Id
                   && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Uri != null ? Uri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)Type;
                return hashCode;
            }
        }

        public AudioIdType IdType { get; }
    }
}

