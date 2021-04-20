using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using Flurl;
using Flurl.Http;
using Google.Protobuf;
using Nito.AsyncEx;
using Spotify.Download.Proto;
using SpotifyLibrary.Helpers;

namespace SpotifyLibrary.Models
{
    public class CdnUrl
    {
        private readonly AsyncLock uriLock = new AsyncLock();
        private readonly ByteString? _fileId;
        private Uri? _url;

        public CdnUrl(ByteString fileId)
        {
            _url = null;
            _fileId = fileId;
        }


        public CdnUrl(Uri url)
        {
            _url = url;
            _fileId = null;
        }

        /// <summary>
        /// Fetches the cdn url from a fileid. If the url is already set this returns the same url
        /// </summary>
        /// <returns></returns>
        public async Task<Uri> Url()
        {
            using (await uriLock.LockAsync())
            {
                if (_url != null) return _url;
                var url = await FetchUrl();
                _url = url;


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
                        Expiration = -1;
                        Debug.WriteLine("Invalid __token__ in CDN url: " + url);
                        return _url;
                    }

                    Expiration = (long)expireAt * 1000;
                }
                else
                {
                    var param = queryDictionary.AllKeys[0];
                    int i = param.IndexOf('_');
                    if (i == -1)
                    {
                        Expiration = -1;
                        Debug.WriteLine("Couldn't extract expiration, invalid parameter in CDN url: " + url);
                        return _url;
                    }
                    int length = i - 0 + 1;
                    Expiration = long.Parse(param.Substring(0, length)) * 1000;
                }

                return _url;
            }
        }

        public long Expiration { get; private set; }

        private async Task<Uri> FetchUrl()
        {
            var resp = await (await ApResolver.GetClosestSpClient())
                .AppendPathSegments("storage-resolve", "files", "audio", "interactive")
                .AppendPathSegments(_fileId!.ToByteArray().BytesToHex())
                .WithOAuthBearerToken((await SpotifyLibrary.Instance.Tokens
                    .GetToken("playlist-read")).AccessToken)
                .GetBytesAsync();

            if (resp == null)
                throw new Exception("Error while getting url");

            var proto = StorageResolveResponse.Parser.ParseFrom(resp);
            if (proto.Result == StorageResolveResponse.Types.Result.Cdn)
            {
                var url = proto.Cdnurl[new Random().Next(proto.Cdnurl.Count - 1)];
                Debug.WriteLine("Fetched CDN url for {0}: {1}", _fileId.ToByteArray().BytesToHex(), url);
                return new Uri(url);
            }
            else
            {
                throw new Exception($"Could not retrieve CDN url! result: {proto.Result}");
            }
        }
    }
}
