using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Enums;

namespace MediaLibrary.Interfaces
{
    public interface IAudioUser
    {
        IAudioId Id { get; }
        AudioItemType Type { get; }
        AudioServiceType Service { get; }
        string DisplayName { get; }
        List<UrlImage> Images { get; }
    }
}
