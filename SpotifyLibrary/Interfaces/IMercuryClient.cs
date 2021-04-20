using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;

namespace SpotifyLibrary.Interfaces
{
    public interface IMercuryClient : IPacketsManager
    {
        ISpotifyConnection Connection { get; }
        string CountryCode { get; }

        /// <summary>
        /// Sends a package (asynchronously) over a TCP connection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="MercuryException"></exception>
        /// <returns></returns>
        Task<T> SendAsync<T>(SystemTextJsonMercuryRequest<T> request, CancellationToken? ct = null) where T : class;

        /// <summary>
        /// Sends a package (asynchronously) over a TCP connection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="MercuryException"></exception>
        /// <returns></returns>
        Task<T> SendAsync<T>(JsonMercuryRequest<T> request, CancellationToken? ct = null) where T : class;

        Task<MercuryResponse?> SendAsync(
            RawMercuryRequest request, CancellationToken? ct = null);
    }
}
