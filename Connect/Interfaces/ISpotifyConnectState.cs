using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using JetBrains.Annotations;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyConnectState
    {
        string ActiveDeviceId { get;  }
        PlayerState ConnectState { get; }
        Task UpdateState([NotNull] PutStateReason reason,
            int playerTime,
            [NotNull] PlayerState state);
    }
}
