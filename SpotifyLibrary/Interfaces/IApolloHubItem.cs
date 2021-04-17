namespace SpotifyLibrary.Interfaces
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
