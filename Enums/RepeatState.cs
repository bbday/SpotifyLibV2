using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibV2.Attributes;

namespace SpotifyLibV2.Enums
{
    public enum RepeatState
    {
        [String("track")] Track,

        [String("context")] Context,

        [String("off")] Off
    }
}