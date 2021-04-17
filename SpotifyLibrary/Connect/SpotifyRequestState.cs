using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Connectstate;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Spotify.Playlist4.Proto;
using SpotifyLibrary.Connect.Interfaces;
using SpotifyLibrary.Enums;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Interfaces;

namespace SpotifyLibrary.Connect
{
    internal class SpotifyRequestState : IRequestListener
    {
        private HttpClient _putclient;
        private string _connectionId;
        private readonly ISpotifyLibrary _library;
        private readonly DeviceInfo _deviceInfo;
        internal readonly SpotifyConnectReceiver ConnectClient;
        private readonly LocalStateWrapper _stateWrapper;

        internal SpotifyRequestState(ISpotifyLibrary library,
            DealerClient dealerClient,
            SpotifyConnectReceiver spotifyConnectReceiver)
        {
            _library = library;
            _deviceInfo = InitializeDeviceInfo();
            ConnectClient = spotifyConnectReceiver;
            PutStateRequest = new PutStateRequest
            {
                MemberType = MemberType.ConnectState,
                Device = new Connectstate.Device
                {
                    DeviceInfo = _deviceInfo
                }
            };
            _stateWrapper = new LocalStateWrapper(this, _library.MercuryClient);
            dealerClient.AddRequestListener(this,
                "hm://connect-state/v1/");
        }
        public PutStateRequest PutStateRequest { get; }

        public RequestResult OnRequest(string mid, int pid, string sender, JObject command)
        {
            throw new NotImplementedException();
        }


        public void NotActive()
        {
            throw new NotImplementedException();
        }
        internal async Task UpdateConnectionId(string conId)
        {
            try
            {
                conId = HttpUtility.UrlDecode(conId, Encoding.UTF8);
            }
            catch (Exception)
            {
                //ignored
            }

            if (_connectionId != null && _connectionId.Equals(conId)) return;

            _connectionId = conId;

            _putclient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            })
            {
                BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
            };
            _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                (await _library.Tokens.GetToken("playlist-read")).AccessToken);
            _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);



            Debug.WriteLine("Updated Spotify-Connection-Id: " + _connectionId);
            _stateWrapper.PlayerState.IsSystemInitiated = true;
            var data = await
                UpdateState(PutStateReason.NewDevice, -1);

            var tryGet =
                Connectstate.Cluster.Parser.ParseFrom(data);
            ConnectClient.OnNewCluster(tryGet);
            if (tryGet?.PlayerState?.Track != null)
                ConnectClient.OnNewPlaybackWrapper(this, tryGet.PlayerState);
        }
        internal async Task<byte[]> UpdateState(PutStateReason reason, int playerTime)
        {
            if (_connectionId == null) throw new ArgumentNullException(_connectionId);

            if (playerTime == -1) PutStateRequest.HasBeenPlayingForMs = 0L;
            else PutStateRequest.HasBeenPlayingForMs = (ulong)playerTime;

            PutStateRequest.PutStateReason = reason;
            PutStateRequest.ClientSideTimestamp = (ulong)TimeProvider.CurrentTimeMillis();
            PutStateRequest.Device.DeviceInfo = _deviceInfo;
            PutStateRequest.Device.PlayerState = _stateWrapper.PlayerState;
            return await PutConnectState(PutStateRequest);
        }
        private async Task<byte[]> PutConnectState(PutStateRequest incomingPutRequest)
        {
            try
            {

                var asBytes = incomingPutRequest.ToByteArray();
                if (_putclient == null)
                {
                    _putclient = new HttpClient(new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip
                    })
                    {
                        BaseAddress = new Uri((await ApResolver.GetClosestSpClient()))
                    };
                    _putclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        (await _library.Tokens.GetToken("playlist-read")).AccessToken);
                    _putclient.DefaultRequestHeaders.Add("X-Spotify-Connection-Id", _connectionId);
                }

                using var ms = new MemoryStream();
                using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(asBytes, 0, asBytes.Length);
                }
                ms.Position = 0;
                var content = new StreamContent(ms);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/protobuf");
                content.Headers.ContentEncoding.Add("gzip");


                var res = await _putclient.PutAsync($"/connect-state/v1/devices/{_library.Configuration.DeviceId}", content);
                if (res.IsSuccessStatusCode)
                {
                    var dt = await res.Content.ReadAsByteArrayAsync();
                    Debug.WriteLine("Put new connect state:");
                    return dt;
                }
                else
                {
                    Debugger.Break();
                    //TODO: error handling
                    return new byte[0];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed updating state.", ex);
                return new byte[0];
            }
        }

        private DeviceInfo InitializeDeviceInfo()
        {
            return new DeviceInfo
            {
                CanPlay = true,
                Volume = 65536,
                Name = _library.Configuration.DeviceName,
                DeviceId = _library.Configuration.DeviceId,
                DeviceType = _library.Configuration.DeviceType,
                DeviceSoftwareVersion = "Spotify-11.1.0",
                SpircVersion = "3.2.6",
                Capabilities = new Capabilities
                {
                    CanBePlayer = true,
                    GaiaEqConnectId = true,
                    SupportsLogout = true,
                    VolumeSteps = 64,
                    IsObservable = true,
                    CommandAcks = true,
                    SupportsRename = false,
                    SupportsPlaylistV2 = true,
                    IsControllable = true,
                    SupportsCommandRequest = true,
                    SupportsTransferCommand = true,
                    SupportsGzipPushes = true,
                    NeedsFullPlayerState = false,
                    SupportedTypes =
                    {
                        "audio/episode",
                        "audio/track"
                    }
                }
            };
        }
    }
}
