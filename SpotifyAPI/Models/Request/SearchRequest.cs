using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using JetBrains.Annotations;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Services.Mercury;

namespace SpotifyLibrary.Models.Request
{
    public struct SearchRequest
    {
        private static int Sequence;
        private readonly string _query;

        public int Limit { get; set; }
        public string ImageSize { get; set; }
        public string Catalogue { get; set; }
        public string Country { get; set; }
        public string Locale { get; set; }
        public string Username { get; set; }
        public SearchRequest(
            [NotNull] string query,
            string imageSize,
            string catalogue,
            string country,
            string locale,
            string name,
            SearchType type,
            int limit = 10)
        {
            this._query = query.Trim();
            Limit = limit;
            ImageSize = imageSize;
            Catalogue = catalogue;
            Country = country;
            Locale = locale;
            Username = name;
            SearchType = type;
        }
        public SearchType SearchType { get; }
        public string BuildUrl()
        {
            switch (SearchType)
            {
                case SearchType.Full:
                    var url =
                        Flurl.Url.Combine(MercurySearchManager.MainSearch,
                            HttpUtility.UrlEncode(_query, Encoding.UTF8));
                    url += "?entityVersion=2";
                    url += "&limit=" + Limit;
                    url += "&imageSize=" + HttpUtility.UrlEncode(ImageSize, Encoding.UTF8);
                    url += "&catalogue=" + HttpUtility.UrlEncode(Catalogue, Encoding.UTF8);
                    url += "&country=" + HttpUtility.UrlEncode(Country, Encoding.UTF8);
                    url += "&locale=" + HttpUtility.UrlEncode(Locale, Encoding.UTF8);
                    url += "&username=" + HttpUtility.UrlEncode(Username, Encoding.UTF8);
                    return url;
                    break;
                case SearchType.Quick:
                    var quickUrl =
                        Flurl.Url.Combine(MercurySearchManager.QuickSearch,
                            HttpUtility.UrlEncode(_query, Encoding.UTF8));
                    quickUrl += "?limit=5";
                    quickUrl += "&intent=2516516747764520149";
                    quickUrl += "&sequence=" + Sequence;
                    quickUrl += "&catalogue=" + HttpUtility.UrlEncode(Catalogue, Encoding.UTF8);
                    quickUrl += "&country=" + HttpUtility.UrlEncode(Country, Encoding.UTF8);
                    quickUrl += "&locale=" + HttpUtility.UrlEncode(Locale, Encoding.UTF8);
                    quickUrl += "&username=" + HttpUtility.UrlEncode(Username, Encoding.UTF8);
                    Interlocked.Increment(ref Sequence);
                    return quickUrl;
                    break;
            }

            throw new NotImplementedException();
        }
    }
}