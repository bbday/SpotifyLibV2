namespace SpotifyLibrary.Models.Response.Mercury.Apollo
{

    public class Header : IApolloHubItem
    {
        public ApolloComponent Component { get; set; }
        public ApolloItemType HubType => ApolloItemType.Header;
        public string Title => Text.Title;
        public ApolloTextObject Text { get; set; }
    }

    public class ApolloTextObject
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }
}
