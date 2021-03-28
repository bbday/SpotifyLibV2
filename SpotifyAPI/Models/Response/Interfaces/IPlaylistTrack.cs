using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibrary.Models.Response.Interfaces
{
    public interface IPlaylistTrack : ITrackItem
    {
        DateTime AddedOn { get;  }
    }
}
