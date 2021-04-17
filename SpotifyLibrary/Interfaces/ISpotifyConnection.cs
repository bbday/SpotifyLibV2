using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface ISpotifyConnection : IDisposable
    {
        ApWelcomeOrFailed? WelcomeOrFailed { get; }
        bool IsConnected { get; }

        /// <summary>
        /// Connects through a TCP Client to the spotify service.
        /// </summary>
        /// <param name="authenticator"></param>
        /// <exception cref="HttpRequestException">Will be thrown when no AccessPoint can be found.</exception>
        /// <exception cref="MercuryUnknownCmdException"></exception>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        Task<ApWelcomeOrFailed> Connect(IAuthenticator authenticator, CancellationToken? ct = null);

        Task Disconnect();
        void Send(MercuryPacketType cmd, byte[] toArray, CancellationToken none);
        string? CountryCode { get; }
    }
}
