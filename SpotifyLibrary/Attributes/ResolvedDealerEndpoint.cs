using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibrary.Attributes
{
    /// <summary>
    ///     Specify a Base URL for an entire Refit interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ResolvedDealerEndpoint
        : Attribute
    {
    }
}