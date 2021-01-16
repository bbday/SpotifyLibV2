using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyConnectClient
    {
        Task<bool> Connect();
    }
}
