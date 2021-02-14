﻿using SpotifyLibV2.Enums;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface ISpotifyConnectReceiver
    {
        void Ready();
        void VolumeChanged(int newVolume);
        void PositionChanged(double newPos);
        void ShuffleStateChanged(bool isShuffle);
        void RepeatStateChanged(RepeatState newState);
        void NewItem(PlayingChangedRequest request);
        void PauseChanged(bool isPaused);
        void Instantiated(ISpotifyConnectClient client);

        void DeviceChanged(string deviceId, SpotDeviceAction reason);
    }

    public enum SpotDeviceAction
    {
        DeviceFocusGot,
        DeviceFocusLost,
        DevicesDisappeared,
        NewDeviceAppeared
    }
}
