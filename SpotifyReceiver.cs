using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2
{
    public class SpotifyReceiver : ISpotifyReceiver
    {

        private readonly IMercuryClient _mercuryClient;
        private readonly ISpotifyStream _stream;
        private readonly CancellationToken _ctx;
        public SpotifyReceiver(
            ISpotifyStream stream, 
            IMercuryClient mercuryClient,
            CancellationToken? ctx = null)
        {
            _stream = stream;
            _mercuryClient = mercuryClient;
            _ctx = ctx ?? (new CancellationTokenSource()).Token;
            var ts = new ThreadStart(BackgroundMethod);
            var backgroundThread = new Thread(ts);
            backgroundThread.Start();
        }

        private void BackgroundMethod()
        {
            Debug.WriteLine("Session.Receiver started");
            while (!_ctx.IsCancellationRequested)
            {
                var packet = _stream.Receive(_ctx); 
                if (!Enum.TryParse(packet.Cmd.ToString(), out MercuryPacketType cmd))
                {
                    Debug.WriteLine(
                        $"Skipping unknown command cmd: {packet.Cmd}, payload: {packet.Payload.BytesToHex()}");
                    continue;
                }

                switch (cmd)
                {
                    case MercuryPacketType.Ping:
                        try
                        {
                            _stream.Send(MercuryPacketType.Pong, packet.Payload, _ctx);
                        }
                        catch (IOException ex)
                        {
                            Debug.WriteLine("Failed sending Pong!", ex);
                        }
                        break;
                    case MercuryPacketType.PongAck:
                        break;
                    case MercuryPacketType.CountryCode:
                        var countryCode = Encoding.Default.GetString(packet.Payload);
                        ListenersHolder.SpotifySessionConcurrentDictionary.ForEach(z =>
                            z.CountryCodeReceived(countryCode));
                        Debug.WriteLine("Received CountryCode: " + countryCode);
                        break;
                    case MercuryPacketType.LicenseVersion:
                        Debug.WriteLine($"Received LicenseVersion: {Encoding.Default.GetString(packet.Payload)}");
                        using (var m = new MemoryStream(packet.Payload))
                        {
                            var id = m.GetShort();
                            if (id != 0)
                            {
                                //var buffer = new byte[m.Get()];
                                //   m.Get(buffer);
                                // Debug.WriteLine(
                                // $"Received LicenseVersion: {id}, {Encoding.Default.GetString(buffer)}");
                            }
                            else
                            {
                                Debug.WriteLine($"Received LicenseVersion: {id}");
                            }
                        }
                        break;
                    case MercuryPacketType.MercuryReq:
                    case MercuryPacketType.MercurySub:
                    case MercuryPacketType.MercuryUnsub:
                    case MercuryPacketType.MercuryEvent:
                        _mercuryClient.Dispatch(packet);
                        break;
                    case MercuryPacketType.ProductInfo:
                        try
                        {
                            ParseProductInfo(packet.Payload);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed parsing prodcut info!" + ex.ToString());
                        }

                        break;

                    case MercuryPacketType.Unknown_0x10:
                        Debug.WriteLine("Received 0x10 : " + packet.Payload.BytesToHex());
                        break;
                }
            }
        }
        private static void ParseProductInfo(byte[] @in)
        {
            //TODO
            Debug.WriteLine(Encoding.Default.GetString(@in));
        }
    }
}
