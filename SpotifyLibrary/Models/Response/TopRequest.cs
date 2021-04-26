using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary;
using Refit;

namespace SpotifyLibrary.Models.Response
{
    public class TopRequest
    {
        public TopRequest(TimeRangeEnum timeRange, int offset = 0, int limit = 20)
        {
           StringAttribute.GetValue(typeof(TimeRangeEnum), timeRange, out var str);
           TimeRange = str ?? "short_term";
           Limit = limit;
           Offset = offset;
        }
        [AliasAs("time_range")]
        public string TimeRange { get; }
        [AliasAs("limit")]
        public int Limit { get; set; } = 20;
        [AliasAs("offset")]
        public int Offset { get; set; } = 0;
    }

    public enum TimeRangeEnum
    {
        [String("long_term")] Year,
        [String("medium_term")] Months,
        [String("short_term")] Month,
    }
}
