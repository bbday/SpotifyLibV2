using System;

namespace SpotifyLibV2.Attributes
{
    /// <summary>
    /// Specify a Base URL for an entire Refit interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ResolvedSpClientEndpoint
        : Attribute
    {
    }
}

