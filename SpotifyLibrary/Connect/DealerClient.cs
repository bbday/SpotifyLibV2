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
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Connect.Models;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Connect
{
    internal class DealerClient : IDisposable
    {
        private readonly IWebsocketClient _webSocket;
        private readonly ITokensProvider _tokensProvider;
        private bool _closed = false;
        private bool _receivedPong = false;

        //private readonly ConcurrentDictionary<PlaylistId, IPlaylistListener>
        //    _playlistListener = new();
        private readonly ConcurrentDictionary<IMessageListener, List<string>> _msgListeners = new();
        private readonly ManualResetEvent _msgListenersLock = new(false);

        internal readonly ConcurrentDictionary<string, IRequestListener> ReqListeners = new();
        private readonly ManualResetEvent _reqListenersLock = new(false);

        public readonly ConcurrentDictionary<string, IPlaylistListener> PlaylistListeners = new();

        private string _userId;
        internal DealerClient(
            string userId,
            ITokensProvider tokenProvider,
            IWebsocketClient wsHandler)
        {
            _userId = userId;
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

            if (!System.Enum.TryParse(obj["type"]?.ToString().ToLower(), out MessageType resolvedType))
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

            foreach (var midprefix in ReqListeners.Keys)
            {
                if (mid.StartsWith(midprefix))
                {
                    var listener = ReqListeners[midprefix];
                    interesting = true;
                    var result =
                        await Task.Run(() => listener.OnRequest(mid, pid, sender, (JObject)command));
                    await SendReply(key, result);
                    Debug.WriteLine("Handled request. key: {0}, result: {1}", key, result);
                }
            }
            if (!interesting)
                Debug.WriteLine("Couldn't dispatch request: " + mid);
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
                else if (headers.Any())
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
                    var playlistRegEx = Regex.Match(uri,
                        @"hm:\/\/playlist\/v2\/playlist\/[\w\d]+");
                    if (playlistRegEx.Success)
                    {
                        var payloadsStr = new string[payloads.Count];
                        for (var i = 0; i < payloads.Count; i++) payloadsStr[i] = payloads[i].ToString();
                        //playlist/collection
                        foreach (var payload in payloadsStr)
                        {
                            lock (PlaylistListeners)
                            {
                                foreach (var listener
                                    in PlaylistListeners)
                                {

                                    var last = uri.Split('/').Last();
                                    if (!DecodeHermesPlaylist(payload, last, out var update)) continue;


                                    //if (listener.Key.Equals("generic") ||
                                    //    listener.Key.Equals(new PlaylistId(update.Playlist.Uri)))
                                    //{
                                    //    //listener.Value.PlaylistUpdate(update);
                                    //}
                                }
                            }
                        }
                    }
                    else
                    {
                        if (uri == "hm://playlist/v2/user/7ucghdgquf6byqusqkliltwc2/rootlist")
                        {
                            var payloadsStr = new string[payloads.Count];
                            for (var i = 0; i < payloads.Count; i++) payloadsStr[i] = payloads[i].ToString();
                            //playlist/collection
                            foreach (var payload in payloadsStr)
                            {
                                var bs = Convert.FromBase64String(payload);
                                var revisionasBase64 = Spotify.Playlist4.Proto.PlaylistModificationInfo.Parser.ParseFrom(bs)
                                    .NewRevision.ToBase64();
                                var revisionid = new RevisionId(revisionasBase64);
                                foreach (var listener
                                    in PlaylistListeners)
                                {
                                    if (listener.Key.Equals("rootlist"))
                                    {
                                        //listener.Value.RootlistUpdate(revisionid);
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

            if (!interesting)
                Debug.WriteLine("Couldn't dispatch message: " + uri);
        }
        async Task SendReply( string key, RequestResult result)
        {
            var success = result == RequestResult.Success;
            var reply =
                $"{{\"type\":\"reply\", \"key\": \"{key.ToLower()}\", \"payload\": {{\"success\": {success.ToString().ToLowerInvariant()}}}}}";
            Debug.WriteLine(reply);
            await _webSocket.SendMessageAsync(reply);
        }
        private static Dictionary<string, string> GetHeaders( JObject obj)
        {
            var headers = obj["headers"] as JObject;
            return headers.ToObject<Dictionary<string, string>>();
        }
        private static bool DecodeHermesPlaylist(string payload,
            string id,
            out object hermes)
        {
            hermes = default;
            return false;
            //var bytes = Convert.FromBase64String(payload);
            //try
            //{
            //    var modification = Spotify.Playlist4.Proto
            //        .PlaylistModificationInfo.Parser.ParseFrom(bytes);
            //    hermes = new HermesPlaylistUpdate(new PlaylistId($"spotify:playlist:{id}"),
            //        modification.Ops);
            //}
            //catch (InvalidProtocolBufferException x)
            //{
            //    Debug.WriteLine(x.ToString());
            //    hermes = null;
            //    return false;
            //}
            //return true;
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
                        $"wss://{(await ApResolver.GetClosestDealerAsync()).Replace("https://", string.Empty)}/?access_token={(await _tokensProvider.GetToken("playlist-read")).AccessToken}"))
                    ;
                return true;
            }
            catch (Exception x)
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

        //public void RemovePlaylistListener(IPlaylistListener listener, PlaylistId uri)
        //{
        //    lock (_playlistListener)
        //    {
        //        if (_playlistListener.ContainsKey(uri))
        //        {
        //            return;
        //        }

        //        _playlistListener.TryRemove(uri, out _);
        //        //_playlistListener.Set();
        //    }
        //}
        //public void AddPlaylistListener( IPlaylistListener listener,
        //     PlaylistId uri)
        //{
        //    lock (_playlistListener)
        //    {
        //        if (_playlistListener.ContainsKey(uri))
        //        {
        //            return;
        //        }

        //        _playlistListener.TryAdd(uri, listener);
        //        //_playlistListener.Set();
        //    }
        //}
        internal void AddMessageListener(IMessageListener listener,  params string[] uris)
        {
            lock (_msgListeners)
            {
                if (_msgListeners.ContainsKey(listener))
                    throw new ArgumentException($"A listener for {Arrays.ToString(uris)} has already been added.");

                _msgListeners.TryAdd(listener, uris.ToList());
                _msgListenersLock.Set();
            }
        }

        internal void RemoveMessageListener( IMessageListener listener)
        {
            lock (_msgListeners)
            {
                _msgListeners.TryRemove(listener, out var ignore);
            }
        }
        internal void AddRequestListener(IRequestListener listener,  string uri)
        {
            lock (ReqListeners)
            {
                if (ReqListeners.ContainsKey(uri))
                    throw new ArgumentException($"A listener for {uri} has already been added.");

                ReqListeners.TryAdd(uri, listener);
                _reqListenersLock.Reset();
            }
        }
        internal void RemoveRequestListener(IRequestListener listener)
        {
            lock (ReqListeners)
            {
                ReqListeners.Values.Remove(listener);
            }
        }
    }
}