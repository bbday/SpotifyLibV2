using System.Collections.Concurrent;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response.Mercury;

namespace SpotifyLibrary.Services.Mercury.Interfaces
{
    public interface IMercuryClient
    {
        SpotifyClient Client { get; }
        SpotifyConnection Connection { get; }
        ConcurrentDictionary<string, string> UserAttributes { get; }
        Task<T> SendAsync<T>([NotNull] SystemTextJsonMercuryRequest<T> request) where T : class;
        Task<T> SendAsync<T>([NotNull] JsonMercuryRequest<T> request) where T : class;
        Task<MercuryResponse> SendAsync(RawMercuryRequest requestRequest);
        void Dispatch(MercuryPacket packet);
        T SendSync<T>(JsonMercuryRequest<T> jsonMercuryRequest) where T : class;
    }
}