using System;
using System.Collections.Generic;
using System.Text;
using Refit;

namespace SpotifyLibV2.Models.Request
{
    public class PlayerVolumeRequest 
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="volumePercent">The volume to set. Must be a value from 0 to 100 inclusive.</param>
        public PlayerVolumeRequest(int volumePercent)
        {
            VolumePercent = volumePercent;
        }

        /// <summary>
        /// The volume to set. Must be a value from 0 to 100 inclusive.
        /// </summary>
        /// <value></value>
        [AliasAs("volume_percent")]
        public int VolumePercent { get; }

        /// <summary>
        /// The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.
        /// </summary>
        /// <value></value>
        [AliasAs("device_id")]
        public string? DeviceId { get; set; }
    }
}
