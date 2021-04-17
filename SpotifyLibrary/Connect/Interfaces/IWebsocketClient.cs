using System;
using System.Threading.Tasks;
using SpotifyLibrary.Connect.Models;

namespace SpotifyLibrary.Connect.Interfaces
{
    public interface IWebsocketClient : IDisposable
    {
        event EventHandler<string> MessageReceived;
        event EventHandler<WebsocketclosedEventArgs> SocketDisconnected;
        event EventHandler<string> SocketConnected;
        event EventHandler<string> SocketIssueOccured;
        Task SendMessageAsync(string jsonContent);
        Task ConnectSocketAsync(Uri connectionUri);
        Task KeepAlive();
    }
}