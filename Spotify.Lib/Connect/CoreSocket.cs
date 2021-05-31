using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Websocket.Client;
using Timer = System.Timers.Timer;

namespace Spotify.Lib.Connect
{
    public class WebSocketKeepAliveConfig
    {
        /// <summary>
        /// The message to send to the server for each keep alive ping
        /// </summary>
        public string PingMessage { get; set; }

        /// <summary>
        /// The frequency at which the keep alive ping is sent
        /// </summary>
        public TimeSpan PingInterval { get; set; }
    }

    public class CoreSocket
    {
        public volatile int Seq;
        private readonly AsyncLock _socketMutex = new AsyncLock();
        private readonly AsyncLock _sendMessageMutex = new AsyncLock();

        private WebsocketClient _messageWebSocket;

        private Timer _keepAliveTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
        private CancellationTokenSource _socketCancellationToken;
        private bool _eventsAttached;
        private Uri _currentUri;

        private bool _disposedValue;

        /// <summary>
        /// Configure this to regularly send a message to the server to keep the socket open
        /// </summary>1`
        public WebSocketKeepAliveConfig WebSocketKeepAlive { get; set; }


        public event EventHandler<string> MessageReceived;
        public event EventHandler<WebsocketclosedEventArgs> SocketDisconnected;
        public event EventHandler<string> SocketConnected;
        public event EventHandler<string> SocketIssueOccured;

        public async Task SendMessageAsync(string message)
        {
            using (await _sendMessageMutex.LockAsync())
            {
                if (string.IsNullOrEmpty(message)) return;

                try
                {
                    _messageWebSocket.Send(message);
                }
                catch (Exception ex)
                {
                    var stack = ex.StackTrace;
                    if (ex is ObjectDisposedException &&
                        stack.Contains("at Windows.Storage.Streams.DataWriter.StoreAsync()"))
                    {
                        // Only a possible fix, not sure if _messageWebSocket or dataWriter is being disposed.
                        // Trying manual dispose call for now
                        await ConnectSocketAsync(_currentUri)
                            .ConfigureAwait(false);
                    }
                    else if (ex.Message.Contains("0x80072EFE"))
                    {
                        // SocketIssueOccured?.Invoke(this, $"${nameof(WebSocketHandler)} error on ${nameof(SendMessageAsync)}: WININET_E_CONNECTION_ABORTED (0x80072EFE). Reconnecting socket. ({ex.Message})");

                        await ConnectSocketAsync(_currentUri);
                    }
                    else
                    {
                        // SocketIssueOccured?.Invoke(this, $"${nameof(WebSocketHandler)} error on ${nameof(SendMessageAsync)}: Failed to send message. ({ex.Message})");
                    }
                }
            }
        }

        public CoreSocket() : base()
        {
            Seq = 0;
        }

        public async Task ConnectSocketAsync(Uri uri)
        {
            if (_disposedValue)
                return;
            Interlocked.Increment(ref Seq);
            using (await _socketMutex.LockAsync())
            {
                _keepAliveTimer.Stop();
                _socketCancellationToken?.Cancel();
                _socketCancellationToken = new CancellationTokenSource();

                _messageWebSocket?.Dispose();
                _messageWebSocket = new WebsocketClient(uri);

                //_messageWebSocket = new MessageWebSocket();
                //_messageWebSocket.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
                //_messageWebSocket.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
                //_messageWebSocket.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36 Edg/85.0.564.41");
                //_messageWebSocket.SetRequestHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");
                //_messageWebSocket.SetRequestHeader("Origin", "https://open.spotify.com");
                //_messageWebSocket.Control.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                //// We send/receive a string, so we need to set the MessageType to Utf8.
                //_messageWebSocket.Control.MessageType = SocketMessageType.Utf8;

                // Events must be set after setting the message type, or an InvalidOperationException will occur
                AttachEvents();

                try
                {
                    await _messageWebSocket.Start()
                        .ConfigureAwait(false);
                    _currentUri = uri;
                }
                catch (Exception ex)
                {
                    // Windows.Web.WebErrorStatus webErrorStatus = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                    SocketIssueOccured?.Invoke(null, $"WebSocket task wrapper threw: ${ex.ToString()}");

                    return;
                }

                if (_keepAliveTimer == null)
                {
                    _keepAliveTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
                }

                _keepAliveTimer.Start();
                SocketConnected?.Invoke(this, null);
            }
        }

        private void AttachEvents()
        {
            if (_eventsAttached)
            {
                //DetachEvents();
            }

            _messageWebSocket.MessageReceived.Subscribe(async msg =>
            {
                using (await _socketMutex.LockAsync())
                {
                    try
                    {

                        //var dataReader = args.RawData;
                        var message = msg.Text;
                        Debug.Write($"incoming ws message; {message}");
                        MessageReceived?.Invoke(this, message);
                    }
                    catch (Exception ex)
                    {
                        SocketIssueOccured?.Invoke(this, $"${nameof(CoreSocket)} error on : {ex.ToString()}. {ex.Message}");
                    }
                }
            });
            _messageWebSocket.DisconnectionHappened.Subscribe(async m =>
            {

                Debug.WriteLine("WS CLOSED: reason: " + m.CloseStatusDescription);

                SocketDisconnected?.Invoke(this, new WebsocketclosedEventArgs());
            });

            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Elapsed += async (sender, o) => { await SendMessageAsync("{\"type\":\"ping\"}"); };
            }

            _eventsAttached = true;
        }

        public Task KeepAlive()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            // Dispose managed state (managed objects).
            if (disposing)
            {
                try
                {
                    _socketCancellationToken?.Cancel();
                }
                catch (Exception)
                {
                    // Ignore errors
                }

                _keepAliveTimer?.Dispose();
                _keepAliveTimer?.Stop();
                _keepAliveTimer = null;

                _socketCancellationToken?.Dispose();
                _messageWebSocket?.Dispose();
            }

            // Free unmanaged resources (unmanaged objects) and  a finalizer below.
            _disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }

    public class WebsocketclosedEventArgs
    {
    }
}