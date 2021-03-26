using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;
using SpotifyLibrary.Models.Response.Interfaces;

namespace SpotifyLibrary.Helpers.JsonConverters
{
    class CustomResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);
            if (objectType == typeof(IAudioItem))
            {
                contract.Converter = new SpotifyItemConverter();
            }
            return contract;
        }
    }
}
