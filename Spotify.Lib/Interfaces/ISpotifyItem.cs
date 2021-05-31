using System.Collections.Generic;
using System.Linq;
using Spotify.Lib.Models;
using Spotify.Lib.Models.Response.SpotItems;
using SpotifyProto;

namespace Spotify.Lib.Interfaces
{
    public interface ISpotifyItem
    {
        string Uri { get;  }
        AudioItemType Type { get; }
        ISpotifyId Id { get; }
        string Name { get;  }
        string Description { get; }
        string Caption { get; }
        List<UrlImage> Images { get; }
    }
}
