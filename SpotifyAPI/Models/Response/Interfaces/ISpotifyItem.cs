using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Models.Response.SpotifyItems;

namespace SpotifyLibrary.Models.Response.Interfaces
{
    public interface ISpotifyItem : IAudioItem
    {
        string Uri { get;  }
    }
}
