using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SpotifyLibrary.Models.Requests
{
    public readonly struct JsonMercuryRequest<T> where T : class
    {
        public readonly RawMercuryRequest Request;

        public JsonMercuryRequest(RawMercuryRequest request)
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