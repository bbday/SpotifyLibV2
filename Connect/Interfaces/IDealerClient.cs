using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibV2.Listeners;

namespace SpotifyLibV2.Connect.Interfaces
{
    public interface IDealerClient
    {
        void Attach();
        void Detach();
        Task<bool> Connect();
        void AddPlaylistListener([NotNull] IPlaylistListener listener,
            [NotNull] string uri);
        void AddMessageListener([NotNull] IMessageListener listener, [NotNull] params string[] uris);
        void RemoveMessageListener([NotNull] IMessageListener listener);
        void AddRequestListener([NotNull] IRequestListener listener, [NotNull] string uri);
        void RemoveRequestListener([NotNull] IRequestListener listener);
        void WaitForListeners();

    }
}
