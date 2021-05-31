using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Spotify.Lib.Models
{
    public readonly struct SystemTextJsonMercuryRequest<T>
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true
        };

        public readonly RawMercuryRequest Request;
        private readonly bool useNetwonsoft;
        public SystemTextJsonMercuryRequest(RawMercuryRequest request, bool usenewtonsoft = false)
        {
            useNetwonsoft = usenewtonsoft;
            Request = request;
        }

        public T Instantiate(MercuryResponse resp)
        {
            try
            {
                var combined = Combine(resp.Payload.ToArray());
                if (typeof(T) == typeof(string))
                    return (T) (object) Encoding.UTF8.GetString(combined);
                if (useNetwonsoft)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(
                        System.Text.Encoding.UTF8.GetString(combined));
                }
                var data = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(combined), jsonSerializerOptions);
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