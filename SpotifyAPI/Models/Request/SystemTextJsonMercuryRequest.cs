using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using SpotifyLibrary.Models.Response;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Models.Request
{
    public readonly struct SystemTextJsonMercuryRequest<T> where T : class
    {
        private static JsonSerializerOptions jsonSerializerOptions = new()
        {
            IgnoreNullValues = true, 
            PropertyNameCaseInsensitive = true
        };
        public readonly RawMercuryRequest Request;

        public SystemTextJsonMercuryRequest([NotNull] RawMercuryRequest request)
        {
            Request = request;
        }

        [CanBeNull]
        public T Instantiate([NotNull] MercuryResponse resp)
        {
            try
            {
                var combined = Combine(resp.Payload.ToArray());
                if (typeof(T) == typeof(string))
                    return (T)(object)Encoding.UTF8.GetString(combined);
                var data = System.Text.Json.JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(combined), jsonSerializerOptions);
                return data;
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.ToString());
                throw;
            }
        }


        public static byte[] Combine(byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }
    }
}