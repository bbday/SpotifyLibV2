using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpotifyLibrary.Connect.Interfaces
{
    internal interface IMessageListener
    {
        Task OnMessage(string uri,Dictionary<string, string> headers,  byte[] payload);
    }
}
