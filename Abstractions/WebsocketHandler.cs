using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibV2.Models;

namespace SpotifyLibV2.Abstractions
{
    public abstract class WebsocketHandler : IDisposable
    {
        public abstract event EventHandler<string> MessageReceived;
        public abstract event EventHandler<WebsocketclosedEventArgs> SocketDisconnected;
        public abstract event EventHandler<string> SocketConnected;
        public abstract event EventHandler<string> SocketIssueOccured;
        public abstract Task SendMessageAsync(string jsonContent);
        public abstract Task ConnectSocketAsync(Uri connectionUri);
        public abstract Task KeepAlive();
        public virtual void Dispose()
        {
        }
    }
}

