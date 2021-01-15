using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using JetBrains.Annotations;
using SpotifyLibV2.Listeners;

namespace SpotifyLibV2.Mercury
{
    public interface IMercuryClient
    {
        void InterestedIn(
            [NotNull] string uri,
            [NotNull] ISubListener listener);

        MercuryResponse Subscribe([NotNull] string uri,
            [NotNull] ISubListener listener);

        void Unsubscribe([NotNull] string uri);
        MercuryResponse SendSync([NotNull] RawMercuryRequest request);

        T SendSync<T>([NotNull] JsonMercuryRequest<T> request) where T : class;
        T SendSync<T>([NotNull] ProtobuffedMercuryRequest<T> request) where T : IMessage<T>;

        int Send([NotNull] RawMercuryRequest request,
            [NotNull] ICallback callback);

        void Dispatch(MercuryPacket packet);
    }
}
