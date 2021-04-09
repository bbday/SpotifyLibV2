using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json;
using SpotifyLibrary.Helpers.JsonConverters;
using SpotifyLibrary.Models.Ids;

namespace SpotifyLibrary.Models.Response.Mercury
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public class Bfs
    {
        [J("id")]
        public string Id { get; set; }
        [J("model")]
        public BfsCardModel Model { get; set; }

        /// <summary>
        /// Possibles types:
        /// <see cref="BfsSectionCarousel"/>
        /// <see cref="BfsSection"/>
        /// <see cref="BfsCard"/>
        /// </summary>
        [J("views", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(BfsConverter))]
        public List<Bfs> Views { get; set; }
    }
    public class BrowseTab
    {
        [J("root")] public Root Root { get; set; }
    }

    public partial class Root
    {
        [J("id")] public string Id { get; set; }

        [J("views")] public List<BfsCell> Views { get; set; }
    }

    public class BfsCell : Bfs
    {

    }

    public class BfsSectionCarousel : Bfs
    {

    }

    public class BfsSection : Bfs
    {

    }

    public class BfsGrid : Bfs
    {

    }


    public class BfsCardModel
    {
        public string Title { get; set; }
        [J("uri")] public string Uri { get; set; }
        [J("name", NullValueHandling = NullValueHandling.Ignore)] public string Name { get; set; }
        [J("image", NullValueHandling = NullValueHandling.Ignore)] public object Image { get; set; }
        [J("description", NullValueHandling = NullValueHandling.Ignore)] public string Description { get; set; }
        [J("meta", NullValueHandling = NullValueHandling.Ignore)] public string Meta { get; set; }
        [J("horizontal", NullValueHandling = NullValueHandling.Ignore)] public bool? Horizontal { get; set; }

        [J("artists", NullValueHandling = NullValueHandling.Ignore)]
        public List<CardArtist> Artists { get; set; }

        [J("subtitle")] public string Subtitle { get; set; }

        public PuffImage PuffImage => Image as PuffImage;
    }
    public class PuffImage
    {
        [J("left")] public string Left { get; set; }
        [J("middle")] public string Middle { get; set; }
        [J("right")] public string Righyt { get; set; }
    }
    public class CardArtist
    {
        [J("name")] public string Name { get; set; }
        [J("uri")] public string Uri { get; set; }
    }
    public class ItemBfs : Bfs
    {
        public AudioService AudioService => AudioService.Spotify;
        public new virtual IAudioId Id { get; }
        public string Uri => Model.Uri;
    }
    public class BfsCardPlaylist : ItemBfs, ISpotifyItem
    {
        private IAudioId _id;

        private List<UrlImage> _images;

        public AudioType Type => AudioType.Playlist;

        public List<UrlImage> Images => _images ??= new List<UrlImage>(1)
        {
            new UrlImage
            {
                Url = Model.Image.ToString()
            }
        };
        public string Name => Model.Name;
        public string Description => Model.Meta;

        public override IAudioId Id => _id ??= new PlaylistId(Uri);
    }
    public class BfsCardAlbum : ItemBfs, ISpotifyItem
    {
        private List<UrlImage> _images;

        private IAudioId _id;
        public AudioType Type => AudioType.Album;

        public List<UrlImage> Images => _images ??= new List<UrlImage>(1)
        {
            new UrlImage
            {
                Url = Model.Image.ToString()
            }
        };
        public string Name => Model.Name;
        public string Description => string.Join($",", Model.Artists.Select(z => z.Name));

        public override IAudioId Id => _id ??= new AlbumId(Uri);
    }
    public class BfsCardShow : ItemBfs, ISpotifyItem
    {
        private List<UrlImage> _images;
        private IAudioId _id;
        public AudioType Type => AudioType.Show;

        public List<UrlImage> Images => _images ??= new List<UrlImage>(1)
        {
            new UrlImage
            {
                Url = Model.Image.ToString()
            }
        };
        public string Name => Model.Name;
        public string Description => Model.Description;
        public override IAudioId Id => _id ??= new ShowId(Uri);
    }
    public class BfsCardLink : ItemBfs, ISpotifyItem
    {
        private List<UrlImage> _images;
        private IAudioId _id;
        public AudioType Type => AudioType.Link;

        public List<UrlImage> Images => _images ??= new List<UrlImage>(1)
        {
            new UrlImage
            {
                Url = Model.Image.ToString()
            }
        };
        public string Name => Model.Title;
        public string Description => string.Empty;
        public override IAudioId Id => _id ??= new LinkId(Uri);

    }

    public class BfsPuff : Bfs
    {

    }
}
