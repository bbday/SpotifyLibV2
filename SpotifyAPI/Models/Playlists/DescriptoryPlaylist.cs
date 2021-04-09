namespace SpotifyLibrary.Models.Playlists
{
    public struct DescriptoryPlaylist
    {
        public DescriptoryPlaylist(Creator madeFor,
            Creator from)
        {
            MadeFor = madeFor;
            MadeBy = from;
        }
        public Creator MadeFor { get; set; }
        public Creator MadeBy { get; set; }
    }
}
