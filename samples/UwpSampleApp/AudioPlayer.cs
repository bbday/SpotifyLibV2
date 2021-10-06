﻿using System;
using System.Threading.Tasks;
using Connectstate;
using LibVLCSharp.Shared;
using SpotifyLib;
using SpotifyLib.Models;
using SpotifyLib.Models.Player;
using UwpSampleApp.Annotations;

namespace UwpSampleApp
{
    public class AudioPlayer : IAudioOutput
    {
        private bool did_set_transfer = false;
        private long transfer_pos = -1;
        public AudioPlayer(SpotifyConfig spotifyConfig)
        {
            DeviceId = spotifyConfig.DeviceId;
            Name = spotifyConfig.DeviceName;
            _libVlc = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new MediaPlayer(_libVlc);

            _mediaPlayer.Playing += (sender, args) =>
            {
                AudioOutputStateChanged?.Invoke(this, SpotifyLib.AudioOutputStateChanged.Playing);
            };
            _mediaPlayer.Paused += (sender, args) =>
            {
                AudioOutputStateChanged?.Invoke(this, SpotifyLib.AudioOutputStateChanged.Paused);
            };

        }

        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mediaPlayer;


        public bool Equals(ISpotifyDevice other)
        {
            return other?.DeviceId == DeviceId;
        }

        public DeviceType Type => DeviceType.Computer;
        public string Name { get; }
        public string DeviceId { get; }
        public bool CanChangeVolume => true;
        public uint Volume { get; }
        public int VolumeSteps { get; }

        private Media _k;
        private StreamMediaInput _m;
        [CanBeNull] public ChunkedStream CurrentStream;

        public Task IncomingStream(ChunkedStream entry)
        {
            CurrentStream?.Dispose();
            _m?.Dispose();
            _k?.Dispose();

            CurrentStream = entry;
            _m = new StreamMediaInput(CurrentStream);
            _k = new Media(_libVlc, _m);
            did_set_transfer = false;

            _mediaPlayer.Play(_k);

            return Task.CompletedTask;
        }

        public async void Resume(long position = -1)
        {
            transfer_pos = position;
            _mediaPlayer.Play();
            //await Task.Delay(50);
            if (position != -1)
            {
                _mediaPlayer.Time = position;
                AudioOutputStateChanged?.Invoke(this, SpotifyLib.AudioOutputStateChanged.ManualSeek);
            }
        }

        public void Pause()
        {
            //  throw new NotImplementedException();
            _mediaPlayer.Pause();
        }

        public event EventHandler<AudioOutputStateChanged> AudioOutputStateChanged;

        public int Position
        {
            get
            {
                if (did_set_transfer)
                {
                    did_set_transfer = true;
                    return (int)transfer_pos;
                }
                return (int) Math.Abs(_mediaPlayer.Time);
            }
        }

        //TODO: Built a local queue somehow...
        public bool CanSkipNext => false;
        public bool CanSkipPrev => false;

        public void SetPos(double d)
        {
            _mediaPlayer.Time = (long) d;
            AudioOutputStateChanged?.Invoke(this, SpotifyLib.AudioOutputStateChanged.ManualSeek);
        }

        public async Task<ChunkedStream> GetCachedStream(SpotifyId playable)
        {
            return null;
        }
    }
}
