using System.Collections.Generic;
using MediaLibrary;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Interfaces;

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

        public AudioServiceType AudioService { get; }
        public IAudioId Id { get; }
        public AudioItemType Type { get; }
        public List<UrlImage> Images { get; }
        public string Name { get; }
        public string Description { get; }
        public string Uri { get; }
        public ApolloItemType HubType { get; }
        public string Title { get; }
    }
}
