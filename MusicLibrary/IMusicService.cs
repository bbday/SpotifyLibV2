using System;
using System.Collections.Generic;
using System.Text;
using MusicLibrary.Interfaces;

namespace MusicLibrary
{
    public interface IMusicService
    {
        IAudioUser CurrentUser { get; }
    }
}
