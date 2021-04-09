namespace SpotifyLibrary.Models.Request
{
    public class ConcertsRequest
    {

        public ConcertsRequest(bool filterByLoc, bool decorate, string locale, string geohash)
        {
            FilterByLoc = filterByLoc;
            Decorate = decorate;
            Locale = locale;
            Geohash = geohash;
        }

        public bool FilterByLoc { get; }
        public bool Decorate { get; }
        public string Locale { get; }
        public string Geohash { get; }
    }
}
