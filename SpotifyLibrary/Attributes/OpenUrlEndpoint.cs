using System;

namespace SpotifyLibrary.Attributes
{
    /// <summary>
    ///     Specify a Base URL for an entire Refit interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class OpenUrlEndpoint
        : Attribute
    {
    }
}