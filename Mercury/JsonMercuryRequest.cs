using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using JetBrains.Annotations;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SpotifyLibV2.Mercury
{
    public class JsonMercuryRequest<T> where T : class
    {
        public readonly RawMercuryRequest Request;
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };
        public JsonMercuryRequest([NotNull] RawMercuryRequest request)
        {
            this.Request = request;
        }
        [NotNull]
        public T Instantiate([NotNull] MercuryResponse resp)
        {
            try
            {
                var combined = Combine(resp.Payload.ToArray());
                if (typeof(T) == typeof(string))
                    return (T) (object) Encoding.UTF8.GetString(combined);
                var data = System.Text.Json.JsonSerializer.Deserialize<T>(combined, jsonSerializerOptions);
                return data ?? throw new InvalidOperationException();
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