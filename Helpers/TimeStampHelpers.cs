using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Helpers
{
    public static class TimeStampHelpers
    {
        public static DateTime UnixTimeStampToDateTime(this 
            long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
