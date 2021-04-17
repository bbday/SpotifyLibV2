using System;
using MediaLibrary.Interfaces;
using Newtonsoft.Json.Serialization;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Helpers
{
    class CustomResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            if (objectType == typeof(IAudioItem))
            {
                contract.Converter = new SpotifyItemConverter();
            }
            //else if (objectType == typeof(IApolloHubItem))
            //{
            //    contract.Converter = new ApolloConverter();
            //}
            return contract;
        }
    }
}
