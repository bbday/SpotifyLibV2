using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyConnectState
    {
        string ActiveDeviceId { get;  }
    }
}
