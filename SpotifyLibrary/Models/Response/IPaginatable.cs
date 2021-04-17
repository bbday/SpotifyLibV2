using System.Collections.Generic;

namespace SpotifyLibrary.Models.Response
{
    public interface IPaginatable<T>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
        string? Next { get; set; }

        IEnumerable<T>? Items { get; set; }
    }

    public interface IPaginatable<T, TNext>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
        string? Next { get; set; }

        IEnumerable<T>? Items { get; set; }
    }
}
