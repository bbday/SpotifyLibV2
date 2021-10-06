using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Connectstate;
using Microsoft.Toolkit.Mvvm.Input;
using Spotify.Metadata.Proto;
using SpotifyLib;
using SpotifyLib.Helpers;
using SpotifyLib.Models;
using SpotifyLib.Models.Player;
using SpotifyLib.Models.Response;
using UwpSampleApp.Annotations;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpSampleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly SpotifyConfig _config;
        private readonly AudioPlayer audioPlayer;
        private string _artist;
        private SpotifyConnectionState _connState;

        private TrackOrEpisode _cur;
        private double _dur;

        private string _image = "ms-appx:///Assets/StoreLogo.png";
        private double _posMs;

        private readonly DispatcherTimer _t;
        private string _title;
        private SpotifyWebsocketState _wsState;

        public ObservableCollection<RemoteSpotifyDevice> AvailableDevices
            = new ObservableCollection<RemoteSpotifyDevice>();

        private bool _isPaused;

        public MainPage()
        {
            InitializeComponent();
            SkipNextCmd = new AsyncRelayCommand(async () =>
            {
                if (IsPlayingOnLocalDevice)
                {

                }
                else
                {
                    var acknowledged = await _wsState.SendCommandToDevice(_wsState.ActiveDevice.DeviceId, new
                    {
                        command = new
                        {
                            endpoint = "skip_next"
                        }
                    });
                }

            }, () => (_wsState != null && _wsState.CanSkipNext) || (audioPlayer != null && audioPlayer.CanSkipNext));
            SkipPrevCommand = new AsyncRelayCommand(async () =>
            {
                if (PositionMs > 30E3)
                {
                    //seek to 0
                    SeekTo(0);
                }
                else
                {
                    if (IsPlayingOnLocalDevice)
                    {
                        //TODO
                    }
                    else
                    {
                        var acknowledged = await _wsState.SendCommandToDevice(_wsState.ActiveDevice.DeviceId, new
                        {
                            command = new
                            {
                                endpoint = "skip_prev"
                            }
                        });
                    }
                }
            }, () => (_wsState != null && (PositionMs > 30E3 || _wsState.CanSkipPrevious))
                     || (audioPlayer != null && (audioPlayer.Position > 30E3 || audioPlayer.CanSkipPrev)));
            _t = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _t.Tick += (sender, o) => { PositionMs += 500; };
            _config = SpotifyConfig.Default();

            audioPlayer = new AudioPlayer(_config);
            audioPlayer.AudioOutputStateChanged += AudioPlayerOnAudioOutputStateChanged;
        }

        public string Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Artist
        {
            get => _artist;
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    OnPropertyChanged(nameof(Artist));
                }
            }
        }

        public string PlayingOnDeviceString
            => _wsState?.ActiveDevice == null
                ? "Not playing on any device."
                : $"Playing on device {_wsState.ActiveDevice.Name}";

        public double PositionMs
        {
            get => _posMs;
            set
            {
                if (Math.Abs(value - _posMs) > 1)
                {
                    _posMs = value;
                    OnPropertyChanged(nameof(PositionMs));
                }
            }
        }

        public double DurationMs
        {
            get => _dur;
            set
            {
                if (Math.Abs(value - _dur) > 1)
                {
                    _dur = value;
                    OnPropertyChanged(nameof(DurationMs));
                }
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (value != _isPaused)
                {
                    _isPaused = value;
                    OnPropertyChanged(nameof(IsPaused));
                }
            }
        }


        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var c = SpotifyConfig.Default();
            var email = Environment.GetEnvironmentVariable("SPOTIFY_EMAIL");
            var password = Environment.GetEnvironmentVariable("SPOTIFY_PASSWORD");
            _connState = await SpotifyClientMethods.Authenticate(
                new UserpassAuthenticator(email,
                    password), _config,
                CancellationToken.None);
            _wsState =
                await
                    SpotifyWebsocketState.ConnectToRemote(_connState, audioPlayer, CancellationToken.None);
            _ = UpdateFromCluster(_wsState.LatestCluster, c);

            _wsState.ClusterUpdated += (o, cluster) =>
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.High, () => { UpdateFromCluster(cluster, c); });
            };
            _wsState.ActiveDeviceChanged += (o, tuple) =>
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.High, () => { OnPropertyChanged(nameof(PlayingOnDeviceString)); });
            };
            _wsState.DevicesAvailableChanged += (o, list) =>
            {
                _ = CoreApplication.MainView.CoreWindow.Dispatcher
                    .RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        AvailableDevices.Clear();
                        foreach (var remoteSpotifyDevice in list) AvailableDevices.Add(remoteSpotifyDevice);
                    });
            };

        }

        private async Task UpdateFromCluster(Cluster cluster, SpotifyConfig c)
        {
            if (cluster.PlayerState.IsPlaying)
            {
                if (cluster.PlayerState.IsPaused)
                {
                    _t.Stop();
                    IsPaused = true;
                }
                else
                {
                    _t.Start();
                    IsPaused = false;
                }
            }

            AvailableDevices.Clear();
            foreach (var remoteSpotifyDevice in cluster.Device)
                AvailableDevices.Add(new RemoteSpotifyDevice(remoteSpotifyDevice.Value, c));

            if (cluster.PlayerState.Track?.Uri == null)
                //TODO: set null..
                return;

            var track = await new SpotifyId(cluster.PlayerState.Track.Uri).FetchAsync<Track>(_connState,
                CancellationToken.None);
            Title = track.Name;
            Artist = string.Join(", ", track.Artist.Select(a => a.Name));
            Image = GetImage(track.Album
                ?.CoverGroup);
            DurationMs = track.Duration;
            PositionMs = GetPosition(cluster.PlayerState);
            OnPropertyChanged(nameof(PlayingOnDeviceString));
            SkipNextCmd.NotifyCanExecuteChanged();
            SkipPrevCommand.NotifyCanExecuteChanged();
        }

        private void AudioPlayerOnAudioOutputStateChanged(object sender, AudioOutputStateChanged e)
        {
            _ = CoreApplication.MainView.CoreWindow.Dispatcher
                .RunAsync(CoreDispatcherPriority.High, () =>
                {
                    switch (e)
                    {
                        case AudioOutputStateChanged.Playing:
                            _t.Start();
                            IsPaused = false;
                            break;
                        case AudioOutputStateChanged.Paused:
                            _t.Stop();
                            IsPaused = true;
                            break;
                    }
                    SkipNextCmd.NotifyCanExecuteChanged();
                    SkipPrevCommand.NotifyCanExecuteChanged();
                    if (!(audioPlayer.CurrentStream?.TrackOrEpisode.Equals(_cur) ?? true))
                    {
                        //changed

                        Image = GetImage(audioPlayer?.CurrentStream?.TrackOrEpisode.Track?.Album
                            ?.CoverGroup);
                        Title = audioPlayer?.CurrentStream?.TrackOrEpisode.Track?.Name;
                        var art = audioPlayer?.CurrentStream?.TrackOrEpisode.Track?.Artist;

                        Artist = art == null ? null : string.Join(", ", art.Select(a => a.Name));
                        DurationMs = audioPlayer?.CurrentStream?.TrackOrEpisode.Track?.Duration ?? 0;
                        PositionMs = audioPlayer?.Position ?? 0;
                        OnPropertyChanged(nameof(PlayingOnDeviceString));
                    }
                });
        }

        private static string GetImage(ImageGroup gr)
        {
            if (gr != null)
            {
                var img = ImageId.GetImages(gr)
                    .FirstOrDefault(a => a.Size == Spotify.Metadata.Proto.Image.Types.Size.Large);
                return $"https://i.scdn.co/image/{img.Uri}";
            }

            return "ms-appx:///Assets/StoreLogo.png";
        }

        private static long GetPosition(PlayerState state)
        {
            var diff = (int)(TimeHelper.CurrentTimeMillisSystem - state.Timestamp);
            return (int)(state.PositionAsOfTimestamp + diff);
        }


        private async void PauseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (PauseButton.IsChecked != null && PauseButton.IsChecked.Value)
            {
                //pause
                if (IsPlayingOnLocalDevice)
                {
                    audioPlayer.Pause();
                }
                else
                {
                    //send command to spotify.
                    var acknowledged = await _wsState.SendCommandToDevice(_wsState.ActiveDevice.DeviceId, new
                    {
                        command = new
                        {
                            endpoint = "pause"
                        }
                    });
                }
            }
            else
            {
                //resume.
                if (IsPlayingOnLocalDevice)
                {
                    audioPlayer.Resume();
                }
                else
                {
                    //send command to spotify
                    var acknowledged = await _wsState.SendCommandToDevice(_wsState.ActiveDevice.DeviceId, new
                    {
                        command = new
                        {
                            endpoint = "resume"
                        }
                    });
                }
            }
        }
        private async Task SeekTo(double slideValue)
        {
            if (IsPlayingOnLocalDevice)
            {
                audioPlayer.SetPos(slideValue);
            }
            else
            {
                var acknowledged = await _wsState.SendCommandToDevice(_wsState.ActiveDevice.DeviceId, new
                {
                    command = new
                    {
                        endpoint = "seek_to",
                        value = slideValue
                    }
                });
            }
        }
        // private bool dragStarted = false;


        public bool IsPlayingOnLocalDevice => _wsState.ActiveDevice.Equals(audioPlayer);

        public AsyncRelayCommand SkipPrevCommand { get; }

        public AsyncRelayCommand SkipNextCmd { get; }

        private bool DragStarted;
        private void TimelineSlider_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Debug.WriteLine($"Seek to :{timelineSlider}");
            SeekTo(timelineSlider.Value);
            DragStarted = false;
        }

        private void TimelineSlider_OnPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (DragStarted)
            {
                SeekTo(timelineSlider.Value);
            }
        }

        private void TimelineSlider_OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            DragStarted = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sel = (sender as ComboBox).SelectedItem;
            if (sel is RemoteSpotifyDevice dev)
            {
                //switch command..
                _wsState.SendTransferCommand(dev.DeviceId);
            }
        }
    }
}