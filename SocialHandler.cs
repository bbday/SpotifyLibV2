using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SpotifyLibV2.Interfaces;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2
{
    internal class SocialHandler : ISubListener
    {
        private readonly ISocialPresence _session;

        internal SocialHandler(ISocialPresence inter)
        {
            _session = inter;
        }

        public void OnEvent(MercuryResponse resp)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(new MemoryStream(Combine(resp.Payload.ToArray())));
            using var jsonTextReader = new JsonTextReader(sr);
            var data = typeof(UserPresence) == typeof(string)
                ? (UserPresence) (object) sr.ReadToEnd()
                : serializer.Deserialize<UserPresence>(jsonTextReader);
            _session.IncomingPresence(data);
        }

        internal static byte[] Combine(byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }
    }
}