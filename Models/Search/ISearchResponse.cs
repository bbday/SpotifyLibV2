namespace SpotifyLibV2.Models.Search
{
    public class StringResponse : ISearchResponse
    {
        public StringResponse(string data)
        {
            Data = data;
        }
        public string Data { get; }
        public SearchType SearchType { get; }
    }
    public interface ISearchResponse
    {
        SearchType SearchType { get; }
    }
    public enum SearchType
    {
        Quick,
        Full
    }
}
