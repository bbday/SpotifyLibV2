using System;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface IPacketsManager : IDisposable
    {
        void Dispatch(MercuryPacket packet);
        void AppendToQueue(MercuryPacket packet);
        void Handle(MercuryPacket packet); 
        void Exception(Exception ex);
    }
}
