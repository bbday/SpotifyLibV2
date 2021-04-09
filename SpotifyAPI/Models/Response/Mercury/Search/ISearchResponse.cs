using SpotifyLibrary.Enum;

namespace SpotifyLibrary.Models.Response.Mercury.Search
{
    public interface ISearchResponse
    {
        SearchType SearchType { get; }
    }
}