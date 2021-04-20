using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary.Interfaces
{
    public interface ICollectionItem : IEquatable<IAudioId>
    {
        public long AddedAtTimeStamp { get; set; }
        public IAudioId ItemId { get; }
        public DateTime AddedAtDate { get; }
    }
}