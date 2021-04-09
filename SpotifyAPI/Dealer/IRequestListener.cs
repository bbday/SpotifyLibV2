using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace SpotifyLibrary.Dealer
{
    public interface IRequestListener
    {
        RequestResult OnRequest([NotNull] string mid, int pid, [NotNull] string sender, [NotNull] JObject command);
        void NotActive();
    }
}
