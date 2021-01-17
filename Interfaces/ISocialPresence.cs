using System.Threading.Tasks;
using SpotifyLibV2.Models.Public;

namespace SpotifyLibV2.Interfaces
{
    public interface ISocialPresence
    {
        Task IncomingPresence(UserPresence presence);
    }
}
