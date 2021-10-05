using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using Spotify.Download.Proto;
using SpotifyLib.Helpers;
using SpotifyLib.Models.Response;

namespace SpotifyLib.Models
{
    public static class CdnUrlExt
    {
        public static async Task<(byte[] Stream, WebHeaderCollection Headers)> GetRequest(this CdnUrl cdnUrl,
            SpotifyConnectionState connState,
            long rangeStart, long rangeEnd)
        {
            var request = WebRequest.Create(await cdnUrl.Url(connState)) as HttpWebRequest;
            Debug.Assert(request != null, nameof(request) + " != null");
            request.AddRange(rangeStart, rangeEnd);

            using var httpWebResponse = request.GetResponse() as HttpWebResponse;
            Debug.Assert(httpWebResponse != null, nameof(httpWebResponse) + " != null");
            if (httpWebResponse.StatusCode != HttpStatusCode.PartialContent)
            {
                throw new Exception($"{httpWebResponse.StatusCode} : {httpWebResponse.StatusDescription}");
            }

            var input = httpWebResponse.GetResponseStream();
            using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            var bt = ms.ToArray();
            ms.Dispose();
            input.Dispose();
            return (bt, httpWebResponse.Headers);
        }
    }

    public class CdnUrl
    {
        private readonly ByteString _fileId;
        private long _expiration;
        private Uri _url;
        public CdnUrl(ByteString fileId, Uri url)
        {
            this._fileId = fileId;
            _url = null;
            _expiration = 0;
            SetUrl(url);
        }
        public void SetUrl(Uri url)
        {
            _url = url;

            if (_fileId != null)
            {
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(url.Query);

                var tokenStr = queryDictionary["__token__"];
                if (tokenStr != null && !tokenStr.IsEmpty())
                {
                    long? expireAt = null;
                    var split = tokenStr.Split('~');
                    foreach (var str in split)
                    {
                        int i = str.IndexOf('=');
                        if (i == -1) continue;
                        int length = i - 0 + 1;
                        string extracted = str.Substring(0, length);
                        if (extracted.Equals("exp="))
                        {
                            expireAt = long.Parse(str.Substring(i + 1));
                            break;
                        }
                    }

                    if (expireAt == null)
                    {
                        _expiration = -1;
                        Debug.WriteLine("Invalid __token__ in CDN url: " + url);
                        return;
                    }

                    _expiration = (long)expireAt * 1000;
                }
                else
                {
                    var param = queryDictionary.AllKeys[0];
                    int i = param.IndexOf('_');
                    if (i == -1)
                    {
                        _expiration = -1;
                        Debug.WriteLine("Couldn't extract expiration, invalid parameter in CDN url: " + url);
                        return;
                    }
                    int length = i - 0 + 1;
                    _expiration = long.Parse(param.Substring(0, length)) * 1000;
                }
            }
            else
            {
                _expiration = -1;
            }
        }
        public async Task<Uri> Url(SpotifyConnectionState connState,
            CancellationToken ct = default)
        {
            if (_expiration == -1) return _url;

            if (_expiration <= TimeHelper.CurrentTimeMillisSystem + TimeSpan.FromMinutes(5).TotalMilliseconds)
            {
                _url = await GetAudioUrl(_fileId, connState, ct);
                SetUrl(_url);
            }
            return _url;
        }
        public bool IsExpired => DateTimeOffset.Now.ToUnixTimeSeconds() > _expiration;


        private static async Task<Uri> GetAudioUrl(ByteString fileId,
            SpotifyConnectionState connState,
            CancellationToken ct = default)
        {
            var resp = await (await ApResolver.GetClosestSpClient())
                .AppendPathSegments("storage-resolve", "files", "audio", "interactive")
                .AppendPathSegments(fileId.ToByteArray().BytesToHex())
                .WithOAuthBearerToken((await connState.GetToken(ct)).AccessToken)
                .GetBytesAsync(ct);

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
    }
    public class CdnException : Exception
    {

        public CdnException(string message) : base(message)
        {
        }

        public CdnException(Exception ex) : base(ex.Message, ex)
        {

        }
    }
}
