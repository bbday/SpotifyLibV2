using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Models.Request
{
    public readonly struct JsonMercuryRequest<T> where T : class
    {
        public readonly RawMercuryRequest Request;

        public JsonMercuryRequest([NotNull] RawMercuryRequest request)
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
                    return (T) (object) Encoding.UTF8.GetString(combined);
                using var stream = new MemoryStream(combined);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var data = JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
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