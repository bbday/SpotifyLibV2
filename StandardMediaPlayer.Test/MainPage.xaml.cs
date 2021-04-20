using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Extensions;
using Google.Protobuf;
using LibVLCSharp.Shared;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Authenticators;
using SpotifyLibrary.Configs;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;
using SpotifyProto;
using Regex = System.Text.RegularExpressions.Regex;
using System.Reactive.Linq;
using Akavache;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace StandardMediaPlayer.Test
{
    public class CachableTrackWithData
    {
        public string TrackData { get; set; }
        public string CdnUrl { get; set; }
        public string FileData { get; set; }
        public string AudioKey { get; set; }
    }
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {

            var filePath = string.Empty;
            var id = new TrackId("spotify:track:3XfCKKcnArQSyJnZGP7ymx");

            var trackBase64 = await BlobCache.UserAccount.GetObject<CachableTrackWithData>($"play-{id.Uri}")
                .Catch(Observable.Return(default(CachableTrackWithData)));

            if (trackBase64 == default(CachableTrackWithData))
            {
                var abc = new SpotifyLibrary.SpotifyLibrary(SpotifyConfiguration.Default(),
                    new UserPassAuthenticator("tak123chris@gmail.com", "h@ll02U2"));
                var trackFetched = await abc.MercuryClient
                    .SendAsync(RawMercuryRequest.Get("hm://metadata/4/track/" +
                                                     id.ToHexId().ToLower() +
                                                     $"?country=jp"));
                var original = Track.Parser.WithDiscardUnknownFields(true).ParseFrom(
                    trackFetched.Value.Payload.SelectMany(z => z).ToArray());
                await BlobCache.UserAccount.InsertObject(id.Uri, original.ToByteString().ToBase64());
                var track = PickAlternativeIfNecessary(original);

                var tryAndGetTest = track
                    .File
                    .FirstOrDefault(z => z.Format == AudioFile.Types.Format.OggVorbis160);

                var cdnUrl = new CdnUrl(tryAndGetTest.FileId);
                //Download file:

                filePath = await StartDownload(await cdnUrl.Url(), track.Name);

                var audioKey = abc.KeyManager.GetAudioKey(track.Gid, tryAndGetTest.FileId);
                var audioKeyAsString = ByteString.CopyFrom(audioKey).ToBase64();

                var cdnUrlAsString = (await cdnUrl.Url()).ToString();
                trackBase64 =  new CachableTrackWithData
                {
                    AudioKey = audioKeyAsString,
                    CdnUrl = cdnUrlAsString,
                    TrackData = track.ToByteString().ToBase64(),
                    FileData =  tryAndGetTest.ToByteString().ToBase64()
                };
                await BlobCache.UserAccount.InsertObject($"play-{id.Uri}", trackBase64);
            }



            // var newStream = new UrlStream(cdnUrl, audioKey);
         
        }

        private async Task<string> StartDownload(Uri e, string name)
        {
            try
            {
                var file = Path.Combine(ApplicationData.Current.LocalFolder.Path, name);
                if (File.Exists(file))
                {
                    return file;
                }

                var destinationFile =
                    await ApplicationData.Current.LocalFolder.CreateFileAsync(name,
                        CreationCollisionOption.FailIfExists);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync(e);
                using (var fs = File.OpenWrite(destinationFile.Path))
                {
                    await response.Content.CopyToAsync(fs);
                }

                return destinationFile.Path;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private async void Player_OnLoaded(object sender, RoutedEventArgs e)
        {
            //var allPossibilities = new List<(Regex Regex, TypeOfCommands Type)>
            //{
            //    (new Regex("([\\w\\s]+) (https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&\\/\\/=]*))"), TypeOfCommands.EmoteOnIdAndUrl),
            //    (new Regex("(\\d+)(,\\s*\\d+)*"), TypeOfCommands.EmoteOnArrays),
            //    (new Regex("abc"), TypeOfCommands.EmoteOnIdAndName)
            //};

            ////var myInputstring = "uclown https://cdn.discordapp.com/emojis/679842627813900323.png?v=1";
            //var inputsArraySeperated = "abc,defg,ahij";
            //foreach (var possibility in allPossibilities)
            //{
            //    var tryMatch = possibility.Regex.Match(inputsArraySeperated);
            //    if (tryMatch.Success)
            //    {
            //        switch (possibility.Type)
            //        {
            //            case TypeOfCommands.EmoteOnIdAndName:
            //                break;
            //            case TypeOfCommands.EmoteOnIdAndUrl:
            //                break;
            //            case TypeOfCommands.EmoteOnArrays:
            //                break;
            //            default:
            //                throw new ArgumentOutOfRangeException();
            //        }
            //    }
            //}

            //var abc = new SpotifyLibrary.SpotifyLibrary(SpotifyConfiguration.Default(),
            //    new UserPassAuthenticator("tak123chris@gmail.com", "h@ll02U2"));

            //var id = new TrackId("spotify:track:6KCEL11alMnIccqpRBrrN1");
            //var trackFetched = await abc.MercuryClient
            //    .SendAsync(RawMercuryRequest.Get("hm://metadata/4/track/" +
            //                                     id.ToHexId().ToLower() +
            //                                     $"?country=jp"));
            //var original = Track.Parser.WithDiscardUnknownFields(true).ParseFrom(
            //    trackFetched.Value.Payload.SelectMany(z => z).ToArray());

            //var track = PickAlternativeIfNecessary(original);
            //var tryAndGetTest = track
            //    .File
            //    .FirstOrDefault(z => z.Format == AudioFile.Types.Format.OggVorbis160 );

            ////var file = audioQualityPicker.GetFile(track.File.ToList());

            //var cdnUrl = new CdnUrl(tryAndGetTest.FileId);

            //var audioKey = abc.KeyManager.GetAudioKey(track.Gid, tryAndGetTest.FileId);

            //var newStream = new UrlStream(cdnUrl, audioKey);
            //var ab = newStream.Skip(0xa7);
            //if (ab != 0xa7) Debugger.Break();
            //IRandomAccessStream randomAccessStream = newStream.AsRandomAccessStream();

            //var k = MediaSource.CreateFromStream(randomAccessStream, "audio/ogg");
            //Player.Source = k;
        }

        public enum TypeOfCommands
        {
            EmoteOnIdAndName,
            EmoteOnIdAndUrl,
            EmoteOnArrays
        }
        private Track PickAlternativeIfNecessary(Track track)
        {
            //var allPossibilities = new List<(Regex Regex, TypeOfCommands Type)>
            //{
            //    (new Regex("([\\w\\s]+) (https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&\\/\\/=]*))"), TypeOfCommands.EmoteOnIdAndUrl),
            //    (new Regex("abc"), TypeOfCommands.EmoteOnArrays),
            //    (new Regex("abc"), TypeOfCommands.EmoteOnIdAndName)
            //};

            //var myInputstring = "abcccc";
            //foreach (var possibility in allPossibilities)
            //{
            //    var tryMatch = possibility.Regex.Match(myInputstring);
            //    if (tryMatch.Success)
            //    {
            //        switch (possibility.Type)
            //        {
            //            case TypeOfCommands.EmoteOnIdAndName:
            //                break;
            //            case TypeOfCommands.EmoteOnIdAndUrl:
            //                break;
            //            case TypeOfCommands.EmoteOnArrays:
            //                break;
            //            default:
            //                throw new ArgumentOutOfRangeException();
            //        }
            //    }
            //}


            if (track.File.Count > 0) return track;

            return track.Alternative.FirstOrDefault(z => z.File.Count > 0);


        }
    }
}
