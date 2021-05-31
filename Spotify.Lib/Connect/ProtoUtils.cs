using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Connectstate;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Helpers;
using Spotify.Lib.Models;
using Spotify.Player.Proto;
using SpotifyProto;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using PlayOrigin = Spotify.Player.Proto.PlayOrigin;
namespace Spotify.Lib.Connect
{
    public static class MetadataItemsExtensions
    {
        public static string GetMetadataOrDefault(this MapField<string, string> mapField, string key, string defaultValue)
        {
            var exists = mapField.ContainsKey(key);
            return exists ? mapField[key] : defaultValue;
        }
    }
    public static class ProtoUtils
    {
        public static Connectstate.PlayOrigin ConvertPlayOrigin(PlayOrigin po)
        {
            if (po == null) return null;

            var builder = new Connectstate.PlayOrigin();

            if (po.HasFeatureIdentifier)
                builder.FeatureIdentifier = po.FeatureIdentifier;
            if (po.HasFeatureVersion)
                builder.FeatureVersion = po.FeatureVersion;
            if (po.HasViewUri)
                builder.ViewUri = po.ViewUri;
            if (po.HasExternalReferrer)
                builder.ExternalReferrer = po.ExternalReferrer;
            if (po.HasReferrerIdentifier)
                builder.ReferrerIdentifier = po.ReferrerIdentifier;
            if (po.HasDeviceIdentifier)
                builder.DeviceIdentifier = po.DeviceIdentifier;

            if (po.FeatureClasses.Count > 0)
                builder.FeatureClasses.AddRange(po.FeatureClasses);
            return builder;
        }

        public static ContextPlayerOptions ConvertPlayerOptions(
            global::Spotify.Player.Proto.ContextPlayerOptions options)
        {
            if (options == null) return null;

            var builder = new ContextPlayerOptions();
            if (options.HasRepeatingContext)
                builder.RepeatingContext = options.RepeatingContext;
            if (options.HasRepeatingTrack)
                builder.RepeatingTrack = options.RepeatingTrack;
            if (options.HasShufflingContext)
                builder.ShufflingContext = options.ShufflingContext;
            return builder;
        }

        public static void CopyOverMetadata(
            Player.Proto.Context from, PlayerState to)
        {
            foreach (var a in from.Metadata)
            {
                to.ContextMetadata[a.Key] = a.Value;
            }
        }

        public static ProvidedTrack ConvertToProvidedTrack(ContextTrack track, string contextUri)
        {
            if (track == null) return null;


            var b = new ProvidedTrack
            {
                Provider = "context"
            };

            if (!string.IsNullOrEmpty(track.Uid))
                b.Uid = track.Uid;

            if (!string.IsNullOrEmpty(track.Uri))
                b.Uri = track.Uri;
            else if (track.HasGid)
            {
                var uriPrefix = PlayableId.InferUriPrefix(contextUri);
                b.Uri =
                    $"{uriPrefix}{Encoding.UTF8.GetString(Base62Test.CreateInstanceWithInvertedCharacterSet().Encode(track.Gid.ToByteArray()))}";
            }

            if (track.Metadata.ContainsKey("album_uri"))
                b.AlbumUri = track.Metadata["album_uri"];
            if (track.Metadata.ContainsKey("artist_uri"))
                b.ArtistUri = track.Metadata["artist_uri"];

            b.Metadata.Add(track.Metadata);
            return b;
        }

        public static void EnrichTrack(
            ProvidedTrack subject, ContextTrack track)
        {
            if (track.HasUri && !object.Equals(subject.Uri, track.Uri))
                throw new Exception("Illegal Argument");

            foreach (var a in track.Metadata)
            {
                subject.Metadata[a.Key] = a.Value;
            }
        }
        public static void EnrichTrack(
            ContextTrack subject,
            ContextTrack track)
        {
            if (subject.HasUri && track.HasUri && !subject.Uri.IsEmpty() && !track.Uri.IsEmpty()
                && !Object.Equals(subject.Uri, track.Uri))
                throw new Exception("Illegal Argument");

            if (subject.HasGid && track.HasGid && !Object.Equals(subject.Gid, track.Gid))
                throw new Exception("Illegal Argument");

            foreach (var a in track.Metadata)
            {
                subject.Metadata[a.Key] = a.Value;
            }
            // subject.Metadata["media.start_position"] = "118631";
        }

        public static List<ContextPage> JsonToContextPages(JArray array) =>
            array.Select(JsonToContextPage).ToList();
        public static ContextPage JsonToContextPage(JToken obj)
        {
            var b = new ContextPage();
            if (obj["next_page_url"] != null)
                b.NextPageUrl = obj["next_page_url"].ToObject<string>();
            if (obj["page_url"] != null)
                b.PageUrl = obj["page_url"].ToObject<string>();
            if (obj["tracks"] is JArray tracks)
            {
                b.Tracks.AddRange(JsonToContextTracks(tracks));
            }

            return b;
        }
        public static List<ContextTrack> JsonToContextTracks(JArray array)
        {
            return array.Select(JsonToContextTrack).ToList();
        }

        public static ContextTrack JsonToContextTrack(JToken obj)
        {
            var b = new ContextTrack();
            if (!string.IsNullOrEmpty(obj["uri"]?.ToString()))
            {
                var uri = obj["uri"].ToString();
                b.Uri = uri;
                try
                {
                    b.Gid = ByteString.CopyFrom(Utils.HexToBytes(PlayableId.FromUri(uri).ToHexId()));
                }
                catch (Exception x)
                {
                    Debug.WriteLine($"Error for id.. {x.Message}");
                }
            }

            if (!string.IsNullOrEmpty(obj["uid"]?.ToString()))
            {
                var uid = obj["uid"].ToString();
                b.Uid = uid;
            }

            //var j = obj["uri"];
            //var i = obj["uid"];
            //var b = new ContextTrack
            //{
            //    Uri = obj["uri"]?.ToObject<string>() ?? "",
            //    Uid = obj["uid"]?.ToObject<string>() ?? ""
            //};
            var z = obj["metadata"]?.ToList<JToken>().Select(x => new MapField<string, string>
            {
                {
                    x.ToObject<JProperty>()?.Name!, x.ToObject<JProperty>()?.Value.ToObject<string>()
                }
            });
            if (z == null) return b;
            foreach (var y in z)
                b.Metadata.Add(y);
            return b;
        }

        public static bool TrackEquals(ContextTrack first, ContextTrack second)
        {
            if (first == null || second == null) return false;
            if (first.Equals(second)) return true;

            if (first.HasUri && !first.Uri.IsEmpty() && second.HasUri && !second.Uri.IsEmpty())
                return first.Uri.Equals(second.Uri);

            if (first.HasGid && second.HasGid)
                return first.Gid.Equals(second.Gid);

            if (first.HasUid && !first.Uid.IsEmpty() && second.HasUid && !second.Uid.IsEmpty())
                return first.Uid.Equals(second.Uid);

            return false;
        }

        public static void PutFilesAsMetadata(ProvidedTrack builder,
            List<AudioFile> files)
        {
            if (files.Count == 0) return;

            var jArray = new JArray();
            foreach (var file in files)
            {
                if (file.HasFormat) jArray.Add(file.Format);
            }

            if (jArray.Count > 0) builder.Metadata["available_file_formats"] = jArray.ToString();
        }

        public static int GetTrackCount(Album album) => album.Disc.Sum(a => a.Track.Count);

        public static ProvidedTrack ToProvidedTrack(ContextTrack track, string contextUri)
        {
            if (track == null) return null;

            var builder = new ProvidedTrack();
            builder.Provider = "context";
            if (track.HasUid) builder.Uid = track.Uid;
            if (track.HasUri && !track.Uri.IsEmpty())
            {
                builder.Uri = track.Uri;
            }
            else if (track.HasGid)
            {
                var uriPrefix = PlayableId.InferUriPrefix(contextUri);
                builder.Uri =
                    $"{uriPrefix}{Encoding.UTF8.GetString(Base62Test.CreateInstanceWithInvertedCharacterSet().Encode(track.Gid.ToByteArray()))}";
            }

            //try
            //{
            //    builder.AlbumUri = track.Metadata["album_uri"];
            //}
            //catch (Exception ignored)
            //{
            //    // ignored
            //}

            //try
            //{
            //    builder.ArtistUri = track.Metadata["artist_uri"];
            //}
            //catch (Exception ignored)
            //{
            //    // ignored
            //}

            builder.Metadata.Add(track.Metadata);

            return builder;
        }
        public static ContextPlayerOptions JsonToPlayerOptions(JObject obj, ContextPlayerOptions old)
        {
            old ??= new ContextPlayerOptions();
            if (obj != null)
            {
                if (obj.ContainsKey("repeating_context"))
                    old.RepeatingContext = obj["repeating_context"].ToObject<bool>();
                if (obj.ContainsKey("repeating_track"))
                    old.RepeatingTrack = obj["repeating_track"].ToObject<bool>();
                if (obj.ContainsKey("shuffling_context"))
                    old.ShufflingContext = obj["shuffling_context"].ToObject<bool>();
            }

            return old;
        }
        public static Connectstate.PlayOrigin JsonToPlayOrigin(JObject obj)
        {
            var pl = new Connectstate.PlayOrigin();

            if (obj.ContainsKey("feauture_identifier"))
                pl.FeatureIdentifier = obj["feauture_identifier"]?.ToString();
            if (obj.ContainsKey("feature_version"))
                pl.FeatureVersion = obj["feature_version"]?.ToString();
            if (obj.ContainsKey("view_uri"))
                pl.ViewUri = obj["view_uri"]?.ToString();
            if (obj.ContainsKey("external_referrer"))
                pl.ExternalReferrer = obj["external_referrer"]?.ToString();
            if (obj.ContainsKey("referrer_identifier"))
                pl.ReferrerIdentifier = obj["referrer_identifier"]?.ToString();
            if (obj.ContainsKey("device_identifier"))
                pl.DeviceIdentifier = obj["device_identifier"]?.ToString();
            return pl;
        }

        public static Player.Proto.Context JsonToContext(JObject obj)
        {
            var c = new Player.Proto.Context();
            if (obj.ContainsKey("uri"))
                c.Uri = obj["uri"]?.ToString();
            if (obj.ContainsKey("url"))
                c.Url = obj["url"]?.ToString();


            var metadata = obj["metadata"];
            if (metadata != null)
            {
                foreach (var key in metadata)
                {
                    c.Metadata.Add(key.Path, key.ToString());
                }
            }

            if (obj.ContainsKey("pages"))
            {
                foreach (var elm in obj["pages"]!)
                {
                    c.Pages.Add(JsonToContextPage(elm));
                }
            }

            return c;
        }
    }
}
