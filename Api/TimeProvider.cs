using System;

namespace SpotifyLibV2.Api
{
    public static class TimeProvider
    {

        private static readonly object OffsetLock = new();
        private static long _offset;

        public static void Init(SpotifySession sess)
        {
         //   _ = UpdateMelody(sess);
        }

        public static long CurrentTimeMillis()
        {
            lock (OffsetLock)
            {
                return CurrentTimeMillisSystem() + _offset;
            }
        }
        private static readonly DateTime Jan1st1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillisSystem()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        public enum Method
        {
            Ntp, Ping, Melody, Manual
        }
    }
}
