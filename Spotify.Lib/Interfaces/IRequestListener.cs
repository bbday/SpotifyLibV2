using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Spotify.Lib.Models;

namespace Spotify.Lib.Interfaces
{
    internal interface IRequestListener
    {
        Task<RequestResult> OnRequest(string mid, int pid, string sender, JObject command);
        void NotActive();
    }
}