using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Audio.Channels
{
    public class ChannelManager : IChannelManager
    {
        public static readonly int CHUNK_SIZE = 2 * 128 * 1024;

    }
}
