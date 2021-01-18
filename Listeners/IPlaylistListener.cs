using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2.Listeners
{
    public interface IPlaylistListener
    {
        Task PlaylistUpdate(HermesPlaylistUpdate update);
    }
}
