using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enums;

namespace SpotifyLibrary.Connect.Interfaces
{
    internal interface IRequestListener
    {
        Task<RequestResult> OnRequest(string mid, int pid, string sender, JObject command);
        void NotActive();
    }
}
