using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Enums;
using MediaLibrary.Interfaces;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Interfaces
{
    public interface ISpotifyConnectReceiver
    {
        event EventHandler<RepeatState> RepeatStateChanged;
        event EventHandler<double> PositionChanged;
        event EventHandler<bool> IsShuffleCHanged;
        event EventHandler<bool> IsPausedChanged;
        event EventHandler<(PlayingItem Item, IRemoteDevice Device)> NewItem; 
        IRemoteDevice ActiveDevice { get; }
        PlayingItem LastReceivedCluster { get; }
    }
}
