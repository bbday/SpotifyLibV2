using System.Collections.Generic;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using MusicLibrary.Models;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Models.Response.Mercury.Apollo;

namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class EmptyItem : ISpotifyItem, IApolloHubItem
    {
        public EmptyItem()
        {
           
        }
        public EmptyItem(JToken jsonObject)
        {
            OriginalObject = jsonObject;
        }
        public JToken OriginalObject { get; }

        public AudioService AudioService { get; }
        public IAudioId Id { get; }
        public AudioType Type { get; }
        public List<UrlImage> Images { get; }
        public string Name { get; }
        public string Description { get; }
        public string Uri { get; }
        public ApolloItemType HubType { get; }
        public string Title { get; }
    }
}
