using Refit;

namespace Spotify.Lib.Models.Request
{
    public class PlaylistRequest
    {

#nullable enable
        [AliasAs("fields")] public string? Fields { get; set; }

        [AliasAs("additional_types")] public string? AdditionalTypes { get; set; }

        [AliasAs("market")] public string? market { get; set; }

#nullable disable
    }
}