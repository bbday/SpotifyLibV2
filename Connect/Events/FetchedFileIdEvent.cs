using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Ids;

namespace SpotifyLibV2.Connect.Events
{
    public class FetchedFileIdEvent : IGenericEvent
    {
        private readonly IPlayableId content;
        private readonly ByteString fileId;

        public FetchedFileIdEvent([NotNull] IPlayableId content,
            [NotNull] ByteString fileId)
        {
            this.content = content;
            this.fileId = fileId;
        }


        public EventBuilder BuildEvent()
        {
            var @event = new EventBuilder(EventType.FETCHED_FILE_ID);
            @event.Append('2').Append('2');
            @event.Append(fileId.ToByteArray().BytesToHex().ToLowerInvariant());
            @event.Append(content.Uri);
            @event.Append('1').Append('2').Append('2');
            return @event;
        }
    }
}
