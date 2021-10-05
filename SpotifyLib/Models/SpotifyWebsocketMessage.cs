using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;

namespace SpotifyLib.Models
{
    public interface ISpotifyWsMsg
    {
        MessageType Type { get; }
    }
    public readonly struct SpotifyWebsocketRequest : ISpotifyWsMsg
    {
        public SpotifyWebsocketRequest(string mid, int pid, string sender, JObject command)
        {
            Mid = mid;
            Pid = pid;
            Sender = sender;
            Command = command;
        }

        public string Mid { get; }
        public int Pid { get; }
        public string Sender { get; }
        public JObject Command { get; }
        public MessageType Type => MessageType.request;
    }

    public readonly struct Ping : ISpotifyWsMsg
    {
        public MessageType Type => MessageType.ping;
    }
    public readonly struct Pong : ISpotifyWsMsg
    {
        public MessageType Type => MessageType.pong;
    }
    public readonly struct SpotifyWebsocketMessage : ISpotifyWsMsg
    {
        public SpotifyWebsocketMessage(string uri, Dictionary<string, string> headers, byte[] payload)
        {
            Uri = uri;
            Headers = headers;
            Payload = payload;
        }

        public byte[] Payload { get; }
        public Dictionary<string, string> Headers { get; }
        public string Uri { get; }
        public MessageType Type => MessageType.request;
    }
    public enum MessageType
    {
        ping,
        pong,
        message,
        request
    }
}
