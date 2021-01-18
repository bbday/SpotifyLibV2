using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using SpotifyLibV2.Abstractions;
using SpotifyLibV2.Api;
using SpotifyLibV2.Connect.Interfaces;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Models;
using SpotifyLibV2.Models.Public;
using SpotifyLibV2.Models.Request;

namespace SpotifyLibV2.Connect
{
    public class DealerClient : IDealerClient, IDisposable
    {
        private readonly WebsocketHandler _webSocket;
        private readonly ITokensProvider _tokensProvider;
        private bool _closed = false;
        private bool _receivedPong = false;

        private readonly ConcurrentDictionary<string, IPlaylistListener>
            _playlistListener = new();
        private readonly ConcurrentDictionary<IMessageListener, List<string>> _msgListeners = new();
        private readonly ManualResetEvent _msgListenersLock = new(false);

        private readonly ConcurrentDictionary<string, IRequestListener> _reqListeners = new();
        private readonly ManualResetEvent _reqListenersLock = new(false);


        public DealerClient(
            ITokensProvider tokenProvider,
            WebsocketHandler wsHandler)
        {
            _tokensProvider = tokenProvider;
            _webSocket = wsHandler;
        }

        private void WebSocketOnSocketConnected(object sender, string e)
        {
            if (_closed)
            {
                Debug.WriteLine("I wonder what happened here... Terminating. closed: {0}", _closed);
                return;
            }

            Debug.WriteLine("Dealer connected! host: {0}", e?.ToString());
        }

        private void WebSocketOnSocketDisconnected(object sender, WebsocketclosedEventArgs e)
        {
           // throw new NotImplementedException();
        }

        private async void WebSocketOnMessageReceivedAsync(object sender, string e)
        {
            var obj = JObject.Parse(e);
           // WaitForListeners();

            if (!Enum.TryParse(obj["type"]?.ToString().ToLower(), out MessageType resolvedType))
                throw new ArgumentOutOfRangeException(nameof(MessageType), "Unknown message received");
            switch (resolvedType)
            {
                case MessageType.ping:
                    break;
                case MessageType.pong:
                    _receivedPong = true;
                    break;
                case MessageType.message:
                    try
                    {
                        HandleMessage(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed handling message: " + obj + " " + ex.ToString());
                    }
                    break;
                case MessageType.request:
                    try
                    {
                        await HandleRequest(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed handling Request: " + obj, ex);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private async Task HandleRequest(JObject obj)
        {
            Debug.Assert(obj != null, nameof(obj) + " != null");
            var mid = obj["message_ident"]?.ToString();
            var key = obj["key"]?.ToString();
            var headers = GetHeaders(obj);
            var payload = obj["payload"];

            using var @in = new MemoryStream();
            using var outputStream = new MemoryStream(Convert.FromBase64String(payload["compressed"].ToString()));
            if (headers["Transfer-Encoding"]?.Equals("gzip") ?? false)
            {
                using var decompressionStream = new GZipStream(outputStream, CompressionMode.Decompress);
                decompressionStream.CopyTo(@in);
                Debug.WriteLine($"Decompressed");
                var jsonStr = Encoding.Default.GetString(@in.ToArray());
                payload = JObject.Parse(jsonStr);

            }

            var pid = payload["message_id"].ToObject<int>();
            var sender = payload["sent_by_device_id"]?.ToString();

            var command = payload["command"];
            Debug.WriteLine("Received request. mid: {0}, key: {1}, pid: {2}, sender: {3}", mid, key, pid, sender);
            var interesting = false;

            foreach (var midprefix in _reqListeners.Keys)
            {
                if (mid.StartsWith(midprefix))
                {
                    var listener = _reqListeners[midprefix];
                    interesting = true;
                    var result = listener.OnRequest(mid, pid, sender, (JObject)command);
                    await SendReply(key, result);
                    Debug.WriteLine("Handled request. key: {0}, result: {1}", key, result);
                }
            }
            if (!interesting) Debug.WriteLine("Couldn't dispatch request: " + mid);
        }

        private void HandleMessage(JObject obj)
        {
            Debug.Assert(obj != null, nameof(obj) + " != null");
            var uri = obj["uri"]?.ToString();
            var headers = GetHeaders(obj);
            var payloads = (JArray)obj["payloads"];
            byte[] decodedPayload = null;
            if (payloads != null)
            {
                if (headers.ContainsKey("Content-Type")
                    && (headers["Content-Type"].Equals("application/json") ||
                        headers["Content-Type"].Equals("text/plain")))
                {
                    if (payloads.Count > 1) throw new InvalidOperationException();
                    decodedPayload = Encoding.Default.GetBytes(payloads[0].ToString());
                }
                else if(headers.Any())
                {
                    var payloadsStr = new string[payloads.Count];
                    for (var i = 0; i < payloads.Count; i++) payloadsStr[i] = payloads[i].ToString();
                    var x = string.Join("", payloadsStr);
                    using var @in = new MemoryStream();
                    using var outputStream = new MemoryStream(Convert.FromBase64String(x));
                    if (headers.ContainsKey("Transfer-Encoding")
                        && (headers["Transfer-Encoding"]?.Equals("gzip") ?? false))
                    {
                        using var decompressionStream = new GZipStream(outputStream, CompressionMode.Decompress);
                        decompressionStream.CopyTo(@in);
                        Debug.WriteLine($"Decompressed");

                    }

                    decodedPayload = @in.ToArray();
                }
                else
                {
                    //    const playlistRegex = /hm:\/\/playlist\/v2\/playlist\/[\w\d]+/i;
                    var playlistRegEx = Regex.Match(uri, @"hm:\/\/playlist\/v2\/playlist\/[\w\d]+");
                    if (playlistRegEx.Success)
                    {
                        var payloadsStr = new string[payloads.Count];
                        for (var i = 0; i < payloads.Count; i++) payloadsStr[i] = payloads[i].ToString();
                        //playlist/collection
                        foreach (var payload in payloadsStr)
                        {
                            lock (_playlistListener)
                            {
                                foreach (var listener
                                    in _playlistListener)
                                {
                                    if (!DecodeHermesPlaylist(payload, out var update)) continue;
                                    if (listener.Key == update.Playlist.Uri)
                                    {
                                        listener.Value.PlaylistUpdate(update);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                decodedPayload = new byte[0];
            }

            var interesting = false;

            lock (_msgListeners)
            {
                foreach (var listener in _msgListeners.Keys)
                {
                    var dispatched = false;
                    var keys = _msgListeners[listener];
                    foreach (var key
                        in
                        keys.Where(key => uri != null && uri.StartsWith(key) && !dispatched))
                    {
                        interesting = true;
                        listener.OnMessage(uri!, headers, decodedPayload);
                        dispatched = true;
                    }
                }
            }

            if (!interesting) Debug.WriteLine("Couldn't dispatch message: " + uri);
        }
        async Task SendReply([NotNull] string key, [NotNull] RequestResult result)
        {
            var success = result == RequestResult.Success;
            var reply =
                $"{{\"type\":\"reply\", \"key\": \"{key.ToLower()}\", \"payload\": {{\"success\": {success.ToString().ToLowerInvariant()}}}}}";
            await _webSocket.SendMessageAsync(reply);
        }
        private static Dictionary<string, string> GetHeaders([NotNull] JObject obj)
        {
            var headers = obj["headers"] as JObject;
            return headers.ToObject<Dictionary<string, string>>();
        }
        private static bool DecodeHermesPlaylist(string payload, out HermesPlaylistUpdate hermes)
        {
            var bytes = Convert.FromBase64String(payload);
            try
            {
                var modification = Spotify.Playlist4.Proto
                    .PlaylistModificationInfo.Parser.ParseFrom(bytes);
                hermes = new HermesPlaylistUpdate(PlaylistId.FromHex(modification.Uri.ToByteArray().BytesToHex()),
                    modification.Ops);
            }
            catch (InvalidProtocolBufferException x)
            {
                Debug.WriteLine(x.ToString());
                hermes = null;
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            Detach();
        }

        public void Attach()
        {
            _webSocket.MessageReceived += WebSocketOnMessageReceivedAsync;
            _webSocket.SocketDisconnected += WebSocketOnSocketDisconnected;
            _webSocket.SocketConnected += WebSocketOnSocketConnected;
        }

        public void Detach()
        {
            _webSocket.MessageReceived -= WebSocketOnMessageReceivedAsync;
            _webSocket.SocketDisconnected -= WebSocketOnSocketDisconnected;
            _webSocket.SocketConnected -= WebSocketOnSocketConnected;
        }


        public async Task<bool> Connect()
        {
            try
            {
                await _webSocket.ConnectSocketAsync(new Uri(
                    $"wss://{(await ApResolver.GetClosestDealerAsync()).Replace("https://", string.Empty)}/?access_token={_tokensProvider.GetToken("playlist-read").AccessToken}"));
                return true;
            }
            catch(Exception x)
            {
                Debug.WriteLine(x.ToString());
                return false;
            }
        }

        public void WaitForListeners()
        {
            lock (_msgListeners)
            {
                if (!_msgListeners.Any()) return;
            }

            try
            {
                _msgListenersLock.WaitOne();
            }
            catch (Exception)
            {
                //ignored
            }
        }
        public void AddMessageListener([NotNull] IMessageListener listener, [NotNull] params string[] uris)
        {
            lock (_msgListeners)
            {
                if (_msgListeners.ContainsKey(listener))
                    throw new ArgumentException($"A listener for {Arrays.ToString(uris)} has already been added.");

                _msgListeners.TryAdd(listener, uris.ToList());
                _msgListenersLock.Set();
            }
        }
        public void AddPlaylistListener([NotNull] IPlaylistListener listener,
            [NotNull] string uri)
        {
            lock (_playlistListener)
            {
                if (_playlistListener.ContainsKey(uri))
                    throw new ArgumentException($"A listener for {uri} has already been added.");

                _playlistListener.TryAdd(uri, listener);
                //_playlistListener.Set();
            }
        }
        public void RemoveMessageListener([NotNull] IMessageListener listener)
        {
            lock (_msgListeners)
            {
                _msgListeners.TryRemove(listener, out var ignore);
            }
        }
        public void AddRequestListener([NotNull] IRequestListener listener, [NotNull] string uri)
        {
            lock (_reqListeners)
            {
                if (_reqListeners.ContainsKey(uri))
                    throw new ArgumentException($"A listener for {uri} has already been added.");

                _reqListeners.TryAdd(uri, listener);
                _reqListenersLock.Reset();
            }
        }
        public void RemoveRequestListener([NotNull] IRequestListener listener)
        {
            lock (_reqListeners)
            {
                _reqListeners.Values.Remove(listener);
            }
        }
    }
}
