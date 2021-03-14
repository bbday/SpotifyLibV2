using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Connect.Transitions
{
    public enum TransitionReason
    {
        trackdone,
        trackerror,
        fwdbtn,
        backbtn,
        endplay,
        playbtn,
        clickrow,
        logout,
        appload,
        remote
    }
}
