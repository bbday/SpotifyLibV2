using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SpotifyLibV2.Enums;
using SpotifyLibV2.Helpers;

namespace SpotifyLibV2.Models
{
    public class GenericSpotifyItem : INotifyPropertyChanged
    {

        ~GenericSpotifyItem()
        {
            //Deconstructor
            /*if (!string.IsNullOrEmpty(Uri))
            {
                if (!SpotifySession.MusicListeners.TryRemove(Uri, out _))
                {
                    Debug.WriteLine($"Failed removing {Uri} from listeners");
                }
            }*/
        }

        public GenericSpotifyItem(AudioType? typeOverride = null)
        {
            Type = typeOverride ?? AudioType.Unknown;
        }
        private string _uri;

        [JsonPropertyName("uri")]
        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri
        {
            get => _uri;
            set
            {
                // SpotifySession.MusicListeners.AddOrUpdate(value, this);
                Id = value.Split(':').LastOrDefault();
                _uri = value;
                if (Type != default(AudioType)) return;
                Type = value.GetAudioType();
            }
        }


        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public AudioType Type { get; set; }

        [JsonPropertyName("id")]
        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id
        {
            get;
            set;
        }
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ContextUri { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public int ContextIndex { get; set; }

        public bool InLibrary
        {
            get => _inLibrary;
            set
            {
                _inLibrary = value;
                OnPropertyChanged(nameof(InLibrary));
            }
        }
        public bool CurrentlyPlaying
        {
            get => _currentlyPlaying;
            set
            {
                _currentlyPlaying = value;
                OnPropertyChanged(nameof(CurrentlyPlaying));
            }
        }
        public bool InFocus
        {
            get => _inFocus;
            set
            {
                _inFocus = value;
                OnPropertyChanged(nameof(InFocus));
            }
        }
        private bool _inLibrary;
        private bool _currentlyPlaying;
        private bool _inFocus;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
