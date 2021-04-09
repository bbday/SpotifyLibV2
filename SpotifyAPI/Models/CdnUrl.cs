using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibrary.Services.Mercury;
using SpotifyLibV2.Helpers;

namespace SpotifyLibrary.Models
{
    public struct CdnUrl
    {
        private readonly ByteString _fileId;
        private long _expiration;
        private Uri _url;

        private readonly ICdnManager _cdnManager;
        public CdnUrl(ICdnManager cdnManager, ByteString fileId, Uri url)
        {
            _cdnManager = cdnManager;
            this._fileId = fileId;
            _url = null;
            _expiration = 0;
            SetUrl(url);
        }

        public bool IsExpired => DateTimeOffset.Now.ToUnixTimeSeconds() > _expiration;

        public async Task<Uri> Url()
        {
            if (_expiration == -1) return _url;

            if (_expiration <= TimeProvider.CurrentTimeMillis() + TimeSpan.FromMinutes(5).TotalMilliseconds)
            {
                _url = await _cdnManager.GetAudioUrl(_fileId);
                SetUrl(_url);
            }
            return _url;
        }

        public void SetUrl([NotNull] Uri url)
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
    }
}
