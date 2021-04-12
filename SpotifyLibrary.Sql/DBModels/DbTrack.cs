using SQLite;

namespace SpotifyLibrary.Sql.DBModels
{
    public class DbTrack
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Title { get; set; }
        public string ArtistsString { get; set; }
        public string GroupString { get; set; }
        public double DurationMs { get; set; }
        public string ImagesUrl { get; set; }
        public bool CanPlay { get; set; }
        public string Type { get; set; }
        public string Service { get; set; }
        public string Uri { get; set; }
    }
}
