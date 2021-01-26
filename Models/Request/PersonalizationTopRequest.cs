using System;
using System.Collections.Generic;
using System.Text;
using Refit;
using SpotifyLibV2.Attributes;

namespace SpotifyLibV2.Models.Request
{
    public class PersonalizationTopRequest
    {
        /// <summary>
        /// The number of entities to return. Default: 20. Minimum: 1. Maximum: 50. For example: limit=2
        /// </summary>
        /// <value></value>
        [AliasAs("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// The index of the first entity to return. Default: 0 (i.e., the first track). Use with limit to get the next set of entities.
        /// </summary>
        /// <value></value>
        [AliasAs("offset")]
        public int? Offset { get; set; }

        /// <summary>
        /// Over what time frame the affinities are computed. Valid values: long_term
        /// (calculated from several years of data and including all new data as it becomes available),
        /// medium_term (approximately last 6 months), short_term (approximately last 4 weeks). Default: medium_term
        /// </summary>
        /// <value></value>
        [AliasAs("time_range")]
        public string? TimeRangeParam { get; set; }

        public enum TimeRange
        {
            [String("long_term")]
            long_term,

            [String("medium_term")]
            medium_term,

            [String("short_term")]
            short_term
        }
    }
}