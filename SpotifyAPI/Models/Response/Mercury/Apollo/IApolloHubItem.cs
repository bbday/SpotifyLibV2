namespace SpotifyLibrary.Models.Response.Mercury.Apollo
{
    public interface IApolloHubItem
    {
        ApolloItemType HubType { get; }
        string Title { get; }
    }

    public enum ApolloItemType
    {
        Header,
        Card
    }
}
