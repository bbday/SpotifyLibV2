namespace SpotifyLibrary.Models.Playlists
{
    public struct Creator
    {
        public Creator(string uri, string displayName)
        {
            Uri = uri;
            Name = displayName;
        }

        public string Uri { get; set; }
        public string Name { get; set; }
    }
}
