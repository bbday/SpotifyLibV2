﻿using System.Collections.Concurrent;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response;

namespace SpotifyLibrary.Services.Mercury.Interfaces
{
    public interface IMercuryClient
    {
        SpotifyConnection Connection { get; }
        ConcurrentDictionary<string, string> UserAttributes { get; }
        Task<T> SendAsync<T>([NotNull] JsonMercuryRequest<T> request) where T : class;
        Task<MercuryResponse> SendAsync(RawMercuryRequest requestRequest);
        void Dispatch(MercuryPacket packet);
        T SendSync<T>(JsonMercuryRequest<T> jsonMercuryRequest) where T : class;
    }
}