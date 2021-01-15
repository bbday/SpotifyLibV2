using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Spotify;
using SpotifyLibV2.Config;
using SpotifyLibV2.Crypto;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2
{
    public interface ISpotifyStream : IDisposable
    {
        bool ConnectToSpotify(ClientHello clientHello, DiffieHellman keys);

        void Authenticate(
            LoginCredentials credentials,
            SpotifyConfiguration config);

        void Send(MercuryPacketType cmd,
            byte[] payload,
            CancellationToken closedToken);

        MercuryPacket Receive(CancellationToken cts);
    }
}
