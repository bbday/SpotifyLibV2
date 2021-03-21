using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Spotify.Download.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLibV2.Api;
using SpotifyLibV2.Audio.Channels;
using SpotifyLibV2.Audio.Decrypt;
using SpotifyLibV2.Audio.Storage;
using SpotifyLibV2.Connect.Events;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Interfaces;
using SpotifyLibV2.Mercury;
using SpotifyProto;

namespace SpotifyLibV2.Audio.Cdn
{
    public class StoredTrackItem
    {
        public StoredTrackItem()
        {

        }
        public StoredTrackItem(byte[] key, ByteString byt, long totalSize,
            int chunks)
        {
            TotalSize = totalSize;
            AudioKeyBase64 = Convert.ToBase64String(key);
            FileIdBase64 = byt.ToBase64();
            Chunks = chunks;
        }
        public string FileIdBase64 { get; set; }
        public string AudioKeyBase64 { get; set; }
        public long TotalSize { get; set; }
        public int Chunks { get; set; }
        [JsonIgnore]
        public byte[] AudioKey => Convert.FromBase64String(AudioKeyBase64);
        [JsonIgnore]
        public ByteString ByteString => ByteString.FromBase64(FileIdBase64);
    }
    public class CdnManager : ICdnManager
    {
        private readonly ISpotifySession _session;
        public CdnManager(ISpotifySession session)
        {
            _session = session;

        }

        public Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, int chunk)
        {
            return GetRequest(cdnUrl, 
                ChannelManager.CHUNK_SIZE * chunk,
                (chunk + 1) * ChannelManager.CHUNK_SIZE - 1);
        }

        public async Task<(byte[] buffer, WebHeaderCollection headers)> GetRequest(CdnUrl cdnUrl, long rangeStart, long rangeEnd)
        {
            var request = WebRequest.Create(await cdnUrl.Url()) as HttpWebRequest;
            Debug.Assert(request != null, nameof(request) + " != null");
            request.AddRange(rangeStart, rangeEnd);

            using var httpWebResponse = request.GetResponse() as HttpWebResponse;
            Debug.Assert(httpWebResponse != null, nameof(httpWebResponse) + " != null");
            if (httpWebResponse.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new Exception($"{httpWebResponse.StatusCode} : {httpWebResponse.StatusDescription}");
            }

            using var input = httpWebResponse.GetResponseStream();
            using var mem = new MemoryStream();
            Debug.Assert(input != null, nameof(input) + " != null");
            input.CopyTo(mem);
            return (mem.ToArray(), httpWebResponse.Headers);
        }

        public async Task<IGeneralAudioStream> CdnFactory(
            IPlayableId id,
            ByteString gid,
            AudioFile file,
            Uri url,
            IHaltListener haltListener)
        {
            //TODO: Cache
            var tryGetItemFromCache = await _session.CacheManager.GetItem<StoredTrackItem>(id.Id, "data.json");
            var firstBuffer = await _session.CacheManager.TryGetChunk(id, 0);
            if (tryGetItemFromCache != default(StoredTrackItem) && firstBuffer.exists)
            {
                var key = tryGetItemFromCache.AudioKey;
                var cdnUrl = new CdnUrl(this, tryGetItemFromCache.ByteString, url);
                return new CdnStream(id,
                    file,
                    url,
                    (int)tryGetItemFromCache.TotalSize,
                    tryGetItemFromCache.Chunks,
                    _session.Configuration,
                    firstBuffer.chunk,
                    false, haltListener,
                    new AudioDecrypt(key),
                    this, cdnUrl, _session.Player, _session.CacheManager);
            }
            else
            {
                long start = StorageFeedHelper.CurrentTimeMillis();
                var key = _session.AudioKeyManager.GetAudioKey(gid, file.FileId);
                int audioKeyTime = (int)(StorageFeedHelper.CurrentTimeMillis() - start);

                var cdnurl = new CdnUrl(this, file.FileId, url);
                var resp = await GetRequest(cdnurl,
                    0,
                    ChannelManager.CHUNK_SIZE - 1);
                var contentRange = resp.headers["Content-Range"];
                if (string.IsNullOrEmpty(contentRange))
                    throw new Exception("Missing Content-Range header");

                var split = contentRange.Split('/');
                var size = int.Parse(split[1]);
                var chunks = (int)Math.Ceiling((float)size / (float)ChannelManager.CHUNK_SIZE);

                var stored = new StoredTrackItem(key, file.FileId, size, chunks);
                await _session.CacheManager.SaveItem(id.Id, $"data.json", stored);
                var decr = new AudioDecrypt(key);
                //decr.DecryptChunk(0, resp.buffer);
                await _session.CacheManager.SaveChunk(id, 0, resp.buffer);
                return new CdnStream(id,
                    file,
                    url,
                    size,
                    chunks,
                    _session.Configuration,
                    resp.buffer,
                    false, haltListener,
                    decr,
                    this, cdnurl, _session.Player, _session.CacheManager);
            }
        }


        public async Task<Uri> GetAudioUrl(ByteString fileId)
        {
            var resp = await(await ApResolver.GetClosestSpClient())
                .AppendPathSegments("storage-resolve", "files", "audio", "interactive")
                .AppendPathSegments(fileId.ToByteArray().BytesToHex())
                .WithOAuthBearerToken(_session.SpotifyApiClient.Tokens.GetToken("playlist-read").AccessToken)
                .GetBytesAsync();

            if (resp == null)
                throw new Exception("Error while getting url");

            var proto = StorageResolveResponse.Parser.ParseFrom(resp);
            if (proto.Result == StorageResolveResponse.Types.Result.Cdn)
            {
                var url = proto.Cdnurl[new Random().Next(proto.Cdnurl.Count - 1)];
                Debug.WriteLine("Fetched CDN url for {0}: {1}", fileId.ToByteArray().BytesToHex(), url);
                return new Uri(url);
            }
            else
            {
                throw new CdnException($"Could not retrieve CDN url! result: {proto.Result}");
            }
        }

        public async Task<Stream> LoadTrack(TrackId id,
            IAudioQualityPicker audioQualityPicker,
            bool preload,
            [CanBeNull] IHaltListener haltListener)
        {

            var trackFetched = _session.SpotifyApiClient.MercuryClient
                .SendSync(RawMercuryRequest.Get("hm://metadata/4/track/" +
                                                id.ToHexId().ToLower() +
                                                $"?country={_session.CountryCode}"));
            var original = Track.Parser.WithDiscardUnknownFields(true).ParseFrom(
                trackFetched.Payload.SelectMany(z => z).ToArray());

            var track = PickAlternativeIfNecessary(original);

            if (track == null)
            {
                var country = _session.CountryCode;
                if (country != null)
                    ContentRestrictedException.CheckRestrictions(country, original.Restriction.ToList());

                Debug.WriteLine("Couldn't find playable track: " + id.ToString());
                throw new FeederException();
            }

            var data = await LoadTrack(id, track, audioQualityPicker, preload, haltListener);

            return data;
        }

        private async Task<Stream> LoadTrack(IPlayableId id, Track track,
            IAudioQualityPicker audioQualityPicker, bool preload, IHaltListener haltListener)
        {
            var file = audioQualityPicker.GetFile(track.File.ToList());
            if (file == null)
            {
                Debug.WriteLine("Couldn't find any suitable audio file, available: " + string.Join(Environment.NewLine,
                    track.File.ToList().Select(z => z.FileId.ToBase64())));
                throw new FeederException();
            }

            var r = await LoadStream(id, file, track, null, preload, haltListener);
            return r;
        }

        private async Task<Stream> LoadStream(IPlayableId id, AudioFile file, Track track, Episode episode, bool preload,
            IHaltListener haltListener)
        {
            if (track == null && episode == null)
                throw new IllegalStateException();

          //  _session.SpotifyApiClient.EventsService.SendEvent(new
              //  FetchedFileIdEvent(track != null ? PlayableId.From(track) : PlayableId.From(episode),
                //    file.FileId).BuildEvent());

            var respString = await _session.CacheManager.GetItem<string>(id.Id, "storageresponsejsonstring.json");
            StorageResolveResponse resp = null;
            if (respString != default(string))
            {
                resp = StorageResolveResponse.Parser.ParseFrom(ByteString.FromBase64(respString));
            }
            else
            {
                resp = await _session.ContentFeeder.ResolveStorageInteractive(file.FileId, preload);
                var dt = resp.ToByteString().ToBase64();
                await _session.CacheManager.SaveItem<string>(id.Id, 
                    "storageresponsejsonstring.json", dt);
            }

            switch (resp.Result)
            {
                case StorageResolveResponse.Types.Result.Cdn:
                    if (track != null)
                    {
                        var k = await CdnFactory(id, track.Gid, file,
                            new Uri(resp.Cdnurl.First()), haltListener).
                            ConfigureAwait(false);
                        return k.Stream();
                    }
                    break;
                case StorageResolveResponse.Types.Result.Storage:
                    break;
                case StorageResolveResponse.Types.Result.Restricted:
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }

            throw new NotImplementedException();
        }

        private Track PickAlternativeIfNecessary([NotNull] Track track)
        {
            if (track.File.Count > 0) return track;

            return track.Alternative.FirstOrDefault(z => z.File.Count > 0);
        }
    }
    public class CdnException : Exception
    {

        public CdnException([NotNull] string message) : base(message)
        {
        }

        public CdnException([NotNull] Exception ex) : base(ex.Message, ex)
        {

        }
    }
}
