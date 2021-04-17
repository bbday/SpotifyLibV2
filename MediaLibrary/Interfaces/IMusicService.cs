using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Enums;

namespace MediaLibrary.Interfaces
{
    public interface IMusicService
    {
        IAudioUser CurrentUser { get; }
        AudioServiceType ServiceType { get; }
    }
}
