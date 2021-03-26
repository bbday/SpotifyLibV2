using System;

namespace SpotifyLibrary.Services.Mercury
{
    public static class TimeProvider
    {
        public enum Method
        {
            Ntp,
            Ping,
            Melody,
            Manual
        }

        private static readonly object OffsetLock = new();
        private static long _offset;
        private static readonly DateTime Jan1st1970 = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static long CurrentTimeMillis()
        {
            lock (OffsetLock)
            {
                return CurrentTimeMillisSystem() + _offset;
            }
        }

        public static long CurrentTimeMillisSystem()
        {
            return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}