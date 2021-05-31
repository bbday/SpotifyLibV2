using System;
using Newtonsoft.Json.Serialization;
using Spotify.Lib.Interfaces;

namespace Spotify.Lib.Helpers
{
    internal class CustomResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (objectType == typeof(ISpotifyItem)) contract.Converter = new SpotifyItemConverter();
            //else if (objectType == typeof(IApolloHubItem))
            //{
            //    contract.Converter = new ApolloConverter();
            //}
            return contract;
        }
    }
}