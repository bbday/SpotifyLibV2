using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLib.Models
{
    public readonly struct SpotifyWebsocketMessage
    {
        public SpotifyWebsocketMessage(string uri, Dictionary<string, string> headers, byte[] payload, MessageType messageType)
        {
            Uri = uri;
            Headers = headers;
            Payload = payload;
            MessageType = messageType;
        }

        public byte[] Payload { get; }
        public Dictionary<string, string> Headers { get; }
        public string Uri { get; }
        public MessageType MessageType { get; }
    }
    public enum MessageType
    {
        ping,
        pong,
        message,
        request
    }
}
