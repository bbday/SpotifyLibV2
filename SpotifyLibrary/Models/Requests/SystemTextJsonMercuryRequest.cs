using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SpotifyLibrary.Models.Requests
{
    public readonly struct SystemTextJsonMercuryRequest<T> where T : class
    {
        private static JsonSerializerOptions jsonSerializerOptions = new()
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };
        public readonly RawMercuryRequest Request;

        public SystemTextJsonMercuryRequest(RawMercuryRequest request)
        {
            Request = request;
        }

        public T Instantiate(MercuryResponse resp)
        {
            try
            {
                var combined = Combine(resp.Payload.ToArray());
                if (typeof(T) == typeof(string))
                    return (T)(object)Encoding.UTF8.GetString(combined);
                var data = System.Text.Json.JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(combined), jsonSerializerOptions);
                resp.Dispose();
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