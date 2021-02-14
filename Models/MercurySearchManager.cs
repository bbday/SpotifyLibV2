using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using JetBrains.Annotations;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Mercury;
using SpotifyLibV2.Models.Search;

namespace SpotifyLibV2.Models
{
    public readonly struct MercurySearchManager
    {
        internal static readonly string MainSearch = "hm://searchview/km/v4/search/";
        internal static readonly string QuickSearch = "hm://searchview/km/v3/suggest/";
        private readonly IMercuryClient _mercury;
        private readonly string _name;
        private readonly string _locale;
        private readonly string _country;
        public MercurySearchManager(
            IMercuryClient mercury,
            string name, 
            string locale,
            string country)
        {
            _locale = locale;
            _mercury = mercury;
            _name = name;
            _country = country;
        }

        public async Task<ISearchResponse> Request(
            [NotNull] SearchRequest req,
            bool outputString)
        {
            if (req.Username.IsEmpty()) req.Username = _name;
            if (req.Country.IsEmpty()) req.Country = _country;
            if (req.Locale.IsEmpty()) req.Locale = _locale;

            if (!outputString)
            {
                var mercury = _mercury;
                switch (req.SearchType)
                {
                    case SearchType.Quick:
                        return await (Task.Run(() => mercury.SendSync(new
                            JsonMercuryRequest<QuickSearch>(
                                RawMercuryRequest.Get(req.BuildUrl())))));
                    case SearchType.Full:
                        return await Task.Run(() => mercury.SendSync(new
                            JsonMercuryRequest<MercurySearchResponse>(
                                RawMercuryRequest.Get(req.BuildUrl()))));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

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