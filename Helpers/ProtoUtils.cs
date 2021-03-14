using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Connectstate;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using SpotifyLibV2.Ids;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;
using PlayOrigin = Spotify.Player.Proto.PlayOrigin;

namespace SpotifyLibV2.Helpers
{
    public class ProtoUtils
    {
        public static bool TrackEquals(ContextTrack first, ContextTrack second)
        {
            if (first == null || second == null) return false;
            if (first == second) return true;

            if (first.HasUri && !first.Uri.IsEmpty() && second.HasUri && !second.Uri.IsEmpty())
                return first.Uri.Equals(second.Uri);

            if (first.HasGid && !second.HasGid)
                return first.Gid.Equals(second.Gid);

            if (first.HasUid && !first.Uid.IsEmpty() && second.HasUid && !second.Uid.IsEmpty())
                return first.Uid.Equals(second.Uid);

            return false;
        }
        public static List<ContextTrack> JsonToContextTracks([NotNull] JArray array)
        {
            return array.Select(JsonToContextTrack).ToList();
        }

        public static void OverrideDefaultValue([NotNull] FieldDescriptor desc, object newDefault)
        {
            foreach (var property in
                TypeDescriptor.GetProperties(desc).Cast<PropertyDescriptor>().Where(property => newDefault != null))
            {
                property.SetValue(desc, newDefault);
            }
        }

        public static ContextTrack JsonToContextTrack([NotNull] JToken obj)
        {
            var j = obj["uri"];
            var i = obj["uid"];
            var b = new ContextTrack
            {
                Uri = obj["uri"]?.ToObject<string>() ?? "",
                Uid = obj["uid"]?.ToObject<string>() ?? ""
            };
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
        public static void EnrichTrack(
            [NotNull] ContextTrack subject, 
            [NotNull] ContextTrack track)
        {
            if (subject.HasUri && track.HasUri && !Object.Equals(subject.Uri, track.Uri))
                throw new Exception("Illegal Argument");

            if (subject.HasGid && track.HasGid && !Object.Equals(subject.Gid, track.Gid))
                throw new Exception("Illegal Argument");

            foreach (var a in track.Metadata)
            {
                subject.Metadata[a.Key] = a.Value;
            }
        }
        public static void EnrichTrack(
            [NotNull] ProvidedTrack subject,
            [NotNull] ContextTrack track)
        {
            if (track.HasUri && !object.Equals(subject.Uri, track.Uri))
                throw new Exception("Illegal Argument");

            foreach (var a in track.Metadata)
            {
                subject.Metadata[a.Key] = a.Value;
            }
        }

        public static Connectstate.PlayOrigin ConvertPlayOrigin([CanBeNull] PlayOrigin po)
        {
            if (po == null) return null;

            var builder = new Connectstate.PlayOrigin();

            if (!string.IsNullOrEmpty(po.FeatureIdentifier))
                builder.FeatureIdentifier = po.FeatureIdentifier;
            if (!string.IsNullOrEmpty(po.FeatureVersion))
                builder.FeatureVersion = po.FeatureVersion;
            if (!string.IsNullOrEmpty(po.ViewUri))
                builder.ViewUri = po.ViewUri;
            if (!string.IsNullOrEmpty(po.ExternalReferrer))
                builder.ExternalReferrer = po.ExternalReferrer;
            if (!string.IsNullOrEmpty(po.ReferrerIdentifier))
                builder.ReferrerIdentifier = po.ReferrerIdentifier;
            if (!string.IsNullOrEmpty(po.DeviceIdentifier))
                builder.DeviceIdentifier = po.DeviceIdentifier;

            builder.FeatureClasses.AddRange(po.FeatureClasses);
            return builder;
        }
        public static ContextPlayerOptions ConvertPlayerOptions(
            [CanBeNull] global::Spotify.Player.Proto.ContextPlayerOptions options)
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

        public static Context JsonToContext(JObject obj)
        {
            var c = new Context();
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
        public static ContextPlayerOptions JsonToPlayerOptions([NotNull] JObject obj,
            [CanBeNull] ContextPlayerOptions old)
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
        public static Connectstate.PlayOrigin JsonToPlayOrigin([NotNull] JObject obj)
        {
            var pl = new Connectstate.PlayOrigin();

            if(obj.ContainsKey("feauture_identifier")) 
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

        public static void CopyOverMetadata(
            [NotNull] Context from,
            [NotNull] PlayerState to)
        {
            foreach (var a in from.Metadata)
            {
                to.ContextMetadata[a.Key] = a.Value;
            }
        }
        public static void CopyOverMetadata(
            [NotNull] ContextTrack from, 
            [NotNull] ContextTrack to)
        {
            foreach (var a in from.Metadata)
            {
                to.Metadata[a.Key] = a.Value;
            }
        }
        public static void CopyOverMetadata([NotNull] ContextTrack from,
            [NotNull] ProvidedTrack to)
        {
            foreach (var a in from.Metadata)
            {
                to.Metadata[a.Key] = a.Value;
            }
        }

        public static void CopyOverMetadata([NotNull] JObject obj, [NotNull] PlayerState to)
        {
            foreach (var a in obj)
            {
                to.ContextMetadata[a.Key] = a.Value.ToObject<string>();
            }
        }

        public static ProvidedTrack ConvertToProvidedTrack([CanBeNull] ContextTrack track, string contextUri)
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
                    $"{uriPrefix}{Base62Test.CreateInstanceWithInvertedCharacterSet().Encode(track.Gid.ToByteArray())}";
            }
            if (track.Metadata.ContainsKey("album_uri"))
                b.AlbumUri = track.Metadata["album_uri"];
            if (track.Metadata.ContainsKey("artist_uri"))
                b.ArtistUri = track.Metadata["artist_uri"];
            return b;
        }

        public static ContextPage JsonToContextPage([NotNull]JToken obj)
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
        private static JObject MapToJson([NotNull] MapField<string, string> map)
        {
            var obj = new JObject();
            foreach (var a in map)
            {
                obj[a.Key] = a.Value;
            }
            return obj;
        }

        public static JObject CraftContextStateCombo(
            [NotNull] PlayerState ps,
            [NotNull] List<ContextTrack> tracks)
        {
            var context = new JObject {["uri"] = ps.ContextUri, ["url"] = ps.ContextUrl};
            context.Add("metadata", MapToJson(ps.ContextMetadata));

            var pages = new JArray();
            context.Add("pages", pages);

            var page = new JObject {["page_url"] = "", ["next_page_url"] = ""};
            var tracksJson = new JArray();
            foreach (var t in tracks)
            {
                tracksJson.Add(TrackToJson(t));
            }
            page.Add("tracks", tracksJson);
            page.Add("metadata", MapToJson(ps.PageMetadata));
            pages.Add(page);


            var state = new JObject();

            var options = new JObject
            {
                ["shuffling_context"] = ps.Options.ShufflingContext,
                ["repeating_context"] = ps.Options.RepeatingContext,
                ["repeating_track"] = ps.Options.RepeatingTrack
            };

            state.Add("options", options);
            state.Add("skip_to", new JObject());
            state.Add("track", TrackToJson(ps.Track));

            var result = new JObject
            {
                {"context", context}, {"state", state}
            };
            return result;
        }


        public static List<ContextPage> JsonToContextPages([NotNull] JArray array) =>
            array.Select(JsonToContextPage).ToList();

        private static JObject TrackToJson([NotNull] ProvidedTrack track)
        {
            var obj = new JObject {["uri"] = track.Uri, ["uid"] = track.Uid};
            obj.Add("metadata", MapToJson(track.Metadata));
            return obj;
        }
        private static JObject TrackToJson([NotNull] ContextTrack track)
        {
            var obj = new JObject
            {
                {
                    "uri", track.Uri
                },
                {
                    "uid", track.Uid
                },
                {
                    "metadata", MapToJson(track.Metadata)
                }
            };
            return obj;
        }
    }
}
