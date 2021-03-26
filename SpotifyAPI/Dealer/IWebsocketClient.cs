using System;
using System.Threading.Tasks;

namespace SpotifyLibrary.Dealer
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