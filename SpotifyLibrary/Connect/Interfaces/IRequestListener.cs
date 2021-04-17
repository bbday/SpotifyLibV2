using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using SpotifyLibrary.Enums;

namespace SpotifyLibrary.Connect.Interfaces
{
    internal interface IRequestListener
    {
        RequestResult OnRequest(string mid, int pid, string sender, JObject command);
        void NotActive();
    }
}
