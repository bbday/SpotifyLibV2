using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using JetBrains.Annotations;
using Spotify.Download.Proto;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Audio.KeyStuff;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Request;
using SpotifyProto;

namespace SpotifyLibrary.Services.Mercury
{
    public class CdnManager : ICdnManager
    {
        private readonly SpotifyClient _session;

        public CdnManager(SpotifyClient session)
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


        public async Task<ByteString> GetAudioKey(
            Track track, 
            AudioFile file)
        {
            var start = TimeProvider.CurrentTimeMillis();
            var key = await Task.Run(() => _session.AudioKeyManager.GetAudioKey(track.Gid, file.FileId));
            var audioKeyTime = (int)(TimeProvider.CurrentTimeMillis() - start);
            return ByteString.CopyFrom(key);
        }

        public async Task<(Track Track, AudioFile File)> GetFile(ISpotifyId spotifyId,
            IAudioQualityPicker audioQualityPicker)
        {
            var trackFetched = await _session.MercuryClient
                .SendAsync(RawMercuryRequest.Get("hm://metadata/4/track/" +
                                                 spotifyId.ToHexId().ToLower() +
                                                 $"?country={_session.CountryCode}"));
            var original = Track.Parser.WithDiscardUnknownFields(true).ParseFrom(
                trackFetched.Payload.SelectMany(z => z).ToArray());


            var track = PickAlternativeIfNecessary(original);

            if (track == null)
            {
                var country = _session.CountryCode;
                if (country != null)
                    ContentRestrictedException.CheckRestrictions(country, original.Restriction.ToList());

                Debug.WriteLine("Couldn't find playable track: " + spotifyId.ToString());
                throw new FeederException();
            }
            var file = audioQualityPicker.GetFile(track.File.ToList());
            if (file == null)
            {
                Debug.WriteLine("Couldn't find any suitable audio file, available: " + string.Join(Environment.NewLine,
                    track.File.ToList().Select(z => z.FileId.ToBase64())));
                throw new FeederException();
            }

            return (track, file);
        }


        public async Task<IGeneralAudioStream> CdnFactory(
          ISpotifyId id,
          ByteString gid,
          AudioFile file,
          Uri url,
          IHaltListener haltListener,
          Track track,
          Episode episode)
        {
            long start = TimeProvider.CurrentTimeMillis();
            var key = _session.AudioKeyManager.GetAudioKey(gid, file.FileId);
            int audioKeyTime = (int)(TimeProvider.CurrentTimeMillis() - start);

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

            var decr = new AudioDecrypt(key);
            //decr.DecryptChunk(0, resp.buffer);
            return new CdnStream(id,
                file,
                url,
                size,
                chunks,
                _session.Config,
                resp.buffer,
                false, haltListener,
                decr,
                this, cdnurl, _session.Player, _session.CacheManager, track, episode);
        }


        public async Task<Uri> GetAudioUrl(ByteString fileId)
        {
            var resp = await (await ApResolver.GetClosestSpClient())
                .AppendPathSegments("storage-resolve", "files", "audio", "interactive")
                .AppendPathSegments(fileId.ToByteArray().BytesToHex())
                .WithOAuthBearerToken((await _session.Tokens.GetToken("playlist-read")).AccessToken)
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

        public async Task<IGeneralAudioStream> LoadTrack(TrackId id,
            IAudioQualityPicker audioQualityPicker,
            bool preload,
            [CanBeNull] IHaltListener haltListener)
        {
            var (track, file) = await GetFile(id, audioQualityPicker);
            var r = await LoadStream(id, 
                file,
                track, 
                null,
                preload, 
                haltListener);
            return r;
        }


        private async Task<IGeneralAudioStream> LoadStream(ISpotifyId id, AudioFile file, Track track, Episode episode, bool preload,
            IHaltListener haltListener)
        {
            if (track == null && episode == null)
                throw new IllegalStateException();


            StorageResolveResponse resp = null;

            resp = await _session.ContentFeeder.ResolveStorageInteractive(file.FileId, preload);

            //await Task.Run(() => _session.SpotifyApiClient.EventsService
            //    .SendEvent(new
            //    FetchedFileIdEvent(id,
            //        file.FileId).BuildEvent()));

            switch (resp.Result)
            {
                case StorageResolveResponse.Types.Result.Cdn:
                    if (track != null)
                    {
                        var k = await CdnFactory(id, track.Gid, file,
                            new Uri(resp.Cdnurl.First()), haltListener, track, episode).
                            ConfigureAwait(false);
                        return k;
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
}
