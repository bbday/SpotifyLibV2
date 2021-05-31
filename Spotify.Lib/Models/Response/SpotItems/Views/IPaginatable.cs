using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spotify.Lib.Models.Response.SpotItems.Views
{
    public interface IPaginatable<T>
    {
        [SuppressMessage("Microsoft.Naming", "CA1716")]
        string? Next { get; set; }

        IEnumerable<T>? Items { get; set; }
    }

    public interface IPaginatable<T, TNext>
    {
        [SuppressMessage("Microsoft.Naming", "CA1716")]
        string? Next { get; set; }

        IEnumerable<T>? Items { get; set; }
    }
}