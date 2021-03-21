using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using SpotifyLibV2.Audio.Decrypt;
using SpotifyLibV2.Helpers.Extensions;
using SpotifyLibV2.Listeners;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2
{
    public class SpotifyReceiver : ISpotifyReceiver
    {
        private ConcurrentDictionary<string, string> _userAttributes;
        private readonly IMercuryClient _mercuryClient;
        private readonly IAudioKey _audioKey;
        private readonly ISpotifyStream _stream;
        private readonly CancellationToken _ctx;
        public SpotifyReceiver(
            ISpotifyStream stream, 
            IMercuryClient mercuryClient,
            ConcurrentDictionary<string, string> userAttributes, IAudioKey audioKey, CancellationToken? ctx = null)
        {
            _userAttributes = userAttributes;
            _audioKey = audioKey;
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
                    case MercuryPacketType.AesKey:
                    case MercuryPacketType.AesKeyError:
                        _audioKey.Dispatch(packet);
                        break;
                }
            }
        }
        private void ParseProductInfo(byte[] @in)
        {
            var productInfoString = Encoding.Default.GetString(@in);
            Debug.WriteLine(productInfoString);
            var xml = new XmlDocument();
            xml.LoadXml(productInfoString);

            var products = xml.SelectNodes("products");
            if (products != null && products.Count > 0)
            {
                var firstItemAsProducts = products[0];

                var product = firstItemAsProducts.ChildNodes[0];

                var properties = product.ChildNodes;
                for (int i = 0; i < properties.Count; i++)
                {
                    var node = properties.Item(i);
                    _userAttributes.AddOrUpdate(node.Name, node.InnerText);
                }
            }
        }
    }
}
