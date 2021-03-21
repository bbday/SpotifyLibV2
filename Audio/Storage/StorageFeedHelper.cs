using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibV2.Audio.Cdn;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Interfaces;
using SpotifyLibV2.Mercury;
using SpotifyProto;

namespace SpotifyLibV2.Audio.Storage
{
    public class StorageFeedHelper
    {
        private static readonly DateTime Jan1St1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }


    }
}