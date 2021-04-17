using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Connectstate;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Connect
{
    internal class LocalStateWrapper
    {
        //private TracksKeeper _tracksKeeper;
        public readonly PlayerState PlayerState;
        private readonly SpotifyRequestState requestState;
        private HttpClient _cuePointsClient;
        private readonly IMercuryClient _mercury;
        internal LocalStateWrapper(SpotifyRequestState requestState,
            IMercuryClient mercury)
        {
            _mercury = mercury;
            this.requestState = requestState;
            PlayerState = InitState(new PlayerState());
        }
        public static PlayerState InitState(PlayerState plstate)
        {
            plstate.PlaybackSpeed = 1.0;
            plstate.SessionId = string.Empty;
            plstate.PlaybackId = string.Empty;
            plstate.Suppressions = new Suppressions();
            plstate.ContextRestrictions = new Connectstate.Restrictions();
            plstate.Options = new ContextPlayerOptions
            {
                RepeatingTrack = false,
                ShufflingContext = false,
                RepeatingContext = false
            };
            plstate.Position = 0;
            plstate.PositionAsOfTimestamp = 0;
            plstate.IsPlaying = false;
            return plstate;
        }

    }
}
