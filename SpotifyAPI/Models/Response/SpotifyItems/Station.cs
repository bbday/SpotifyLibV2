using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Response.Mercury.Apollo;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class Station : ISpotifyItem, IApolloHubItem
    {
        private readonly JObject _item;
        private string _title;
        private IAudioId _stationId;
        private string _description;
        private string _uri;
        private List<UrlImage> _images;
        public Station(JObject item)
        {
            _item = item;
        }
        public string Title => _title ??= _item["text"]["title"].ToString();


        public List<UrlImage> Images => _images ??= new List<UrlImage>(1)
        {
            new UrlImage
            {
                Url = _item["images"]["main"]["uri"].ToString()
            }
        };
        public string Name => _title ??= _item["text"]["title"].ToString();
        public string Description => _description ??= _item["text"]["subtitle"].ToString();
        public string Uri => _uri ??= _item["id"].ToString();


        public ApolloItemType HubType => ApolloItemType.Card;
        public AudioService AudioService => AudioService.Spotify;
        public IAudioId Id => _stationId ??= new StationId(Uri, _item);
        public AudioType Type => AudioType.Station;
    }
}
