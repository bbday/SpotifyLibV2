namespace SpotifyLibrary.Models.Playlists
{
    public struct BlueBubble
    {
        public BlueBubble(int entries)
        {
            Description = $"{entries} new entries";
        }
        public string Description { get; set; }
    }
}

