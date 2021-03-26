namespace SpotifyLibrary.Models
{
    public readonly struct PlaybackItemWrapper
    {
        public PlaybackItemWrapper(string itemUri)
        {
            ItemUri = itemUri;
        }

        public string ItemUri { get; }
    }
}
