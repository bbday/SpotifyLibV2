using System;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models.Requests;

namespace SpotifyLibrary.Models
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

        public Task<string> RequestAsString(
            SearchRequest req,
            bool outputString,
            CancellationToken cts = default(CancellationToken))
        {
            if (req.Username.IsEmpty()) req.Username = _name;
            if (req.Country.IsEmpty()) req.Country = _country;
            if (req.Locale.IsEmpty()) req.Locale = _locale;

            var mercury = _mercury;
            switch (req.SearchType)
            {
                case SearchType.Quick:
                    return Task.Run<string>(() => (mercury.SendAsync(new
                        SystemTextJsonMercuryRequest<string>(
                            RawMercuryRequest.Get(req.BuildUrl())))), cts);
                case SearchType.Full:
                    return Task.Run<string>(() => mercury.SendAsync(new
                        SystemTextJsonMercuryRequest<string>(
                            RawMercuryRequest.Get(req.BuildUrl()))), cts);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task<T> Request<T>(
            SearchRequest req,
            bool outputString,
            CancellationToken cts = default(CancellationToken)) where T : class, ISearchResponse
        {
            if (req.Username.IsEmpty()) req.Username = _name;
            if (req.Country.IsEmpty()) req.Country = _country;
            if (req.Locale.IsEmpty()) req.Locale = _locale;

            if (!outputString)
            {
                var mercury = _mercury;
                switch (req.SearchType)
                {
                    //case SearchType.Quick:
                    //    return Task.Run<ISearchResponse>(() => (mercury.SendSync(new
                    //        JsonMercuryRequest<QuickSearch>(
                    //            RawMercuryRequest.Get(req.BuildUrl())))), cts);
                    case SearchType.Full:
                        return Task.Run<T>(() => mercury.SendAsync(new
                            JsonMercuryRequest<T>(
                                RawMercuryRequest.Get(req.BuildUrl()))), cts);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                var test = _mercury.SendAsync(new
                    JsonMercuryRequest<string>(
                        RawMercuryRequest.Get(req.BuildUrl())));
                throw new NotImplementedException();
            }
        }
    }
}
