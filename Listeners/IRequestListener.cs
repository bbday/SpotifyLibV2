using System;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SpotifyLibV2.Models.Request;

namespace SpotifyLibV2.Listeners
{
    public interface IRequestListener
    {
        RequestResult OnRequest([NotNull] string mid, int pid, [NotNull] String sender, [NotNull] JObject command);
    }
}
