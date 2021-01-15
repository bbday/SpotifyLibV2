using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpotifyLibV2.Mercury
{
    public class JsonMercuryRequest<T> where T : class
    {
        public readonly RawMercuryRequest Request;

        public JsonMercuryRequest([NotNull] RawMercuryRequest request)
        {
            this.Request = request;
        }

        [NotNull]
        public T Instantiate([NotNull] MercuryResponse resp)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(
                new MemoryStream(Combine(resp.Payload.ToArray())));
            using var jsonTextReader = new JsonTextReader(sr);
            var data =  typeof(T) == typeof(string)
                ? (T)(object)sr.ReadToEnd()
                : serializer.Deserialize<T>(jsonTextReader);
            resp.Dispose();
            return data ?? throw new InvalidOperationException();
        }

        public static byte[] Combine(byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }
    }
}