using SQLite;

namespace SpotifyLibrary.Sql.DBModels
{
    public class DbAlbum
    {
        [PrimaryKey] public string Id { get; set; }
        public string Title { get; set; }
        public string ArtistsString { get; set; }
        public string Uri { get; set; }
        public string Type { get; set; }
        public string Service { get; set; }
        public string TracksString { get; set; }
    }
}