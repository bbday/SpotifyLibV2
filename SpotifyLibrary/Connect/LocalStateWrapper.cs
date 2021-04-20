using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using Google.Protobuf;
using MediaLibrary.Enums;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Audio.Context;
using SpotifyLibrary.Audio.Restrictions;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Ids;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;

namespace SpotifyLibrary.Connect
{
    internal class LocalStateWrapper
    {
        private TracksKeeper? _tracksKeeper;
        public readonly PlayerState PlayerState;
        private readonly SpotifyRequestState requestState;
        private HttpClient _cuePointsClient;
        private readonly IMercuryClient _mercury;

        internal LocalStateWrapper(SpotifyRequestState requestState,
            IMercuryClient mercury)
        {
            _mercury = mercury;
            this.requestState = requestState;
            PlayerState = InitState(new PlayerState());

            requestState.ConnectClient.Player.StateChanged += async (state, metadata) =>
            {
                if (!metadata.HasValue) throw new ArgumentNullException(nameof(metadata));

                switch (state)
                {
                    case MediaPlaybackState.VolumeChanged:
                        //normalized so 0 to 1..
                        // volume/max = norm
                        //volume = norm * max
                        var volume = requestState.ConnectClient.Player.NormalizedVolume *
                                     requestState.ConnectClient.Player.VolumeMax;
                        requestState.PutStateRequest.Device.DeviceInfo.Volume = (uint)volume;
                        requestState.UpdateVolumeInDeviceInfo(volume);
                        await Updated();
                        break;
                    case MediaPlaybackState.None:
                        break;
                    case MediaPlaybackState.Buffering:
                        break;
                    case MediaPlaybackState.FinishedLoading:
                        requestState.FinishedLoading(metadata.Value);
                        break;
                    case MediaPlaybackState.TrackStarted:
                        SetState(true, false, false);
                        await Updated();
                        break;
                    case MediaPlaybackState.TrackPaused:
                        SetState(true, true, false);
                        await Updated();
                        break;
                    case MediaPlaybackState.TrackPlayed:
                        break;
                    case MediaPlaybackState.NewTrack:
                        break;
                    case MediaPlaybackState.PositionChanged:
                        SetPosition((int)requestState.ConnectClient.Player.Position.TotalMilliseconds);
                        await Updated();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            };
        }
        public static PlayerState InitState(PlayerState plstate)
        {
            plstate.PlaybackSpeed = 1.0;
            plstate.SessionId = string.Empty;
            plstate.PlaybackId = string.Empty;
            plstate.Suppressions = new Suppressions();
            plstate.ContextRestrictions = new Connectstate.Restrictions();
            plstate.Options = new ContextPlayerOptions
            {
                RepeatingTrack = false,
                ShufflingContext = false,
                RepeatingContext = false
            };
            plstate.Position = 0;
            plstate.PositionAsOfTimestamp = 0;
            plstate.IsPlaying = false;
            return plstate;
        }
        public Task Updated(bool push = false)
        {
            //UpdateRestrictions();
            // if (push)
            return requestState.UpdateState(PutStateReason.PlayerStateChanged,
                (int)requestState.ConnectClient.Player.Position.TotalMilliseconds);
        }

        private void SetState(bool playing, bool paused, bool buffering)
        {
            if (paused && !playing) throw new ArgumentNullException();
            else if (buffering && !playing) throw new ArgumentNullException();

            var wasPaused = IsPaused;
            PlayerState.IsPlaying = playing;
            PlayerState.IsPaused = paused;
            PlayerState.IsBuffering = buffering;

            if (wasPaused && !paused) // Assume the position was set immediately before pausing
                SetPosition(PlayerState.PositionAsOfTimestamp);
        }
        internal void EnrichWithMetadata(TrackOrEpisode metadata)
        {
            if (metadata.track != null)
            {
                var track = metadata.track;
                if (PlayerState.Track == null) throw new NotImplementedException();

                if (track.HasDuration) _tracksKeeper.UpdateTrackDuration(track.Duration);

                var b = PlayerState.Track;

                if (track.HasPopularity) b.Metadata["popularity"] = track.Popularity.ToString();
                if (track.HasExplicit) b.Metadata["is_explicit"] = track.Explicit.ToString().ToLower();
                if (track.HasName) b.Metadata["title"] = track.Name;
                if (track.HasDiscNumber) b.Metadata["album_disc_number"] = track.DiscNumber.ToString();
                for (var i = 0; i < track.Artist.Count; i++)
                {
                    var artist = track.Artist[i];
                    if (artist.HasName) b.Metadata["artist_name" + (i == 0 ? "" : (":" + i))] = artist.Name;
                    if (artist.HasGid)
                        b.Metadata["artist_uri" + (i == 0 ? "" : (":" + i))] =
                            ArtistId.FromHex(artist.Gid.ToByteArray().BytesToHex()).Uri;
                }

                if (track.Album != null)
                {
                    var album = track.Album;
                    if (album.Disc.Count > 0)
                    {
                        b.Metadata["album_track_count"] = album.Disc.Select(z => z.Track).Count().ToString();
                        b.Metadata["album_disc_count"] = album.Disc.Count.ToString();
                    }

                    if (album.HasName) b.Metadata["album_title"] = album.Name;
                    if (album.HasGid)
                        b.Metadata["album_uri"] =
                            AlbumId.FromHex(album.Gid.ToByteArray().BytesToHex()).Uri;

                    for (int i = 0; i < album.Artist.Count; i++)
                    {
                        var artist = album.Artist[i];
                        if (artist.HasName)
                            b.Metadata["album_artist_name" + (i == 0 ? "" : (":" + i))] = artist.Name;
                        if (artist.HasGid)
                            b.Metadata["album_artist_uri" + (i == 0 ? "" : (":" + i))] =
                                ArtistId.FromHex(artist.Gid.ToByteArray().BytesToHex()).Uri;
                    }

                    if (track.HasDiscNumber)
                    {
                        b.Metadata["album_track_number"] =
                            album.Disc.SelectMany(z => z.Track).ToList().FindIndex(k => k.Gid == track.Gid) +
                            1.ToString();
                    }

                    if (album.CoverGroup?.Image != null
                        && album.CoverGroup.Image.Count > 0)
                        ImageId.PutAsMetadata(b, album.CoverGroup);
                }


                var k = new JArray();
                foreach (var j in track.File
                    .Where(z => z.HasFormat))
                {
                    k.Add(j.Format.ToString());
                }

                b.Metadata["available_file_formats"] = k.ToString();
                PlayerState.Track = b;
            }
        }

        internal void SetBuffering(bool b)
        {
            SetState(true, PlayerState.IsPaused, b);
        }
        internal void SetPlaybackId(string playbackId) => PlayerState.PlaybackId = playbackId;
        internal void SetPosition(long pos)
        {
            PlayerState.Timestamp = TimeProvider.CurrentTimeMillis();
            PlayerState.PositionAsOfTimestamp = pos;
            PlayerState.Position = 0L;
        }
        public int GetPosition()
        {
            int diff = (int)(TimeProvider.CurrentTimeMillis() - PlayerState.Timestamp);
            return (int)(PlayerState.PositionAsOfTimestamp + diff);
        }
        public bool IsPaused => PlayerState.IsPlaying && PlayerState.IsPaused;
        public bool IsShufflingContext => PlayerState.Options.ShufflingContext;
        public bool IsRepeatingContext => PlayerState.Options.RepeatingContext;
        public bool IsRepeatingTrack => PlayerState.Options.RepeatingTrack;
        public string SessionId => PlayerState.SessionId;
        public string ContextUrl => PlayerState.ContextUrl;
        public AbsSpotifyContext? Context { get; private set; }
        public PagesLoader? Pages { get; internal set; }
        public ISpotifyId GetPlayableItem => _tracksKeeper == null
            ? null
            : PlayableId.From(PlayerState.Track);
        private static string GenerateSessionId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            var str = Base64UrlEncode(bytes);
            return str;
        }
        internal static string GeneratePlaybackId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            bytes[0] = 1;
            return bytes.BytesToHex().ToLowerInvariant();
        }
        private static string Base64UrlEncode(byte[] inputBytes)
        {
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-') // replace URL unsafe characters with safe ones
                .Replace('/', '_') // replace URL unsafe characters with safe ones
                .Replace("=", ""); // no padding
        }

        public async Task<string> Transfer(TransferState cmd)
        {
            var ps = cmd.CurrentSession;

            PlayerState.PlayOrigin = ProtoUtils.ConvertPlayOrigin(ps.PlayOrigin);
            PlayerState.Options = ProtoUtils.ConvertPlayerOptions(cmd.Options);

            var sessionId = SetContext(ps.Context);
            var pb = cmd.Playback;

            try
            {
                _tracksKeeper!.InitializeFrom(list =>
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var track = list[i];
                        if ((track.HasUid && ps.CurrentUid.Equals(track.Uid)) ||
                            ProtoUtils.TrackEquals(track, pb.CurrentTrack))
                        {
                            return i;
                        }
                    }

                    return -1;
                }, pb.CurrentTrack, cmd.Queue);
            }
            catch (IllegalStateException ex)
            {
                Debug.WriteLine("Failed initializing tracks, falling back to start. uid: {0}", ps.CurrentUid);
                _tracksKeeper!.InitializeStart();
            }

            PlayerState.PositionAsOfTimestamp = pb.PositionAsOfTimestamp;

            if (pb.IsPaused) PlayerState.Timestamp = TimeProvider.CurrentTimeMillis();
            else PlayerState.Timestamp = pb.Timestamp;

            //await LoadTransforming();
            return sessionId;
        }
        public async Task<string> LoadOnDevice(
            int duration,
            PagedRequest context,
            int? seekTo = null)
        {
            PlayerState.PlayOrigin = new Connectstate.PlayOrigin
            {
                FeatureIdentifier = "harmony",
                FeatureVersion = "4.13.0-6c0e4f7"
            };
            PlayerState.Options = new ContextPlayerOptions
            {
                RepeatingContext = context.RepeatContext,
                RepeatingTrack = context.RepeatTrack,
                ShufflingContext = context.Shuffle
            };

            var ctx = new Context
            {
                Uri = context.ContextUri,
                Restrictions = new Spotify.Player.Proto.Restrictions(),
                Loading = false,
            };
            ctx.Metadata.Add("zelda.context_uri", context.ContextUri);
            var newPage = new ContextPage
            {
                Loading = false, 
                PageUrl = string.Empty,
                NextPageUrl = string.Empty
            };
            newPage.Tracks.AddRange(context.Uris.Select(a => new ContextTrack
              {
                  Uri = a,
                  Gid = ByteString.CopyFrom(Utils.HexToBytes(PlayableId.FromUri(a).ToHexId()))
              }));
            ctx.Pages.Add(newPage);

            var sessionId = SetContext(ctx);

            _tracksKeeper!.InitializeFrom(list =>
            {
                if (context.PlayIndex < list.Count) return (int)context.PlayIndex;
                return -1;
            }, null, null);

            _tracksKeeper.UpdateTrackDuration(duration);
            PlayerState.PositionAsOfTimestamp = 0;

            PlayerState.Timestamp = TimeProvider.CurrentTimeMillis();

            if (seekTo != null) SetPosition((long)seekTo);
            else SetPosition(0);
            return sessionId;
        }
        private string SetContext(Context ctx)
        {
            var uri = ctx.Uri;
            Context = AbsSpotifyContext.From(uri);
            PlayerState.ContextUri = uri;

            if (!Context.IsFinite())
            {
                SetRepeatingContext(false);
                SetShufflingContext(false);
            }

            if (ctx.HasUrl) PlayerState.ContextUrl = ctx.Url;
            else PlayerState.ContextUrl = string.Empty;

            PlayerState.ContextMetadata.Clear();

            ProtoUtils.CopyOverMetadata(ctx, PlayerState);

            Pages = PagesLoader.From(requestState.ConnectClient.Library.MercuryClient, ctx);
            _tracksKeeper = new TracksKeeper(this, Context, _mercury);


            requestState.SetIsActive(true);

            return RenewSessionId();
        }    private string RenewSessionId()
        {
            var sessionId = GenerateSessionId();
            PlayerState.SessionId = sessionId;
            return sessionId;
        }
        public void SetRepeatingContext(bool value)
        {
            if (Context == null) return;

            PlayerState.Options.RepeatingContext =
                true && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_CONTEXT);
        }

        public void SetRepeatingTrack(bool value)
        {
            if (Context == null) return;
            PlayerState.Options.RepeatingTrack =
                true && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_TRACK);
        }
        public void SetShufflingContext(bool value)
        {
            if (Context == null
                || _tracksKeeper == null) return;

            var old = IsShufflingContext;
            PlayerState.Options.ShufflingContext =
                value && Context.Restrictions.Can(RestrictionsManager.Action.SHUFFLE);

            if (old != IsShufflingContext)
                _tracksKeeper.ToggleShuffle(IsShufflingContext);

        }

        public async Task<string> Load(JObject obj)
        {
            var k = (PlayCommandHelper.GetPlayOrigin(obj) as JObject);
            PlayerState.PlayOrigin = ProtoUtils.JsonToPlayOrigin(k!);
            PlayerState.Options =
                ProtoUtils.JsonToPlayerOptions((JObject)PlayCommandHelper.GetPlayerOptionsOverride(obj),
                    PlayerState.Options);

            var sessionId = SetContext(ProtoUtils.JsonToContext((JObject)
                PlayCommandHelper.GetContext(obj)));

            var trackUid = PlayCommandHelper.GetSkipToUid(obj);
            var trackUri = PlayCommandHelper.GetSkipToUri(obj);
            var trackIndex = PlayCommandHelper.GetSkipToIndex(obj);

            if (trackUri != null)
            {
                _tracksKeeper.InitializeFrom(list => list.FindIndex(z =>
                        z.Uri == trackUri),
                    null,
                    null);
            }
            else if (trackUid != null)
            {
                _tracksKeeper.InitializeFrom(list => list.FindIndex(z => z.Uid == trackUid), null, null);
            }
            else if (trackIndex != null)
            {
                _tracksKeeper.InitializeFrom(list =>
                {
                    if (trackIndex < list.Count) return (int)trackIndex;
                    return -1;
                }, null, null);
            }
            else
            {
                _tracksKeeper.InitializeStart();
            }

            var seekTo = PlayCommandHelper.GetSeekTo(obj);
            if (seekTo != null) SetPosition((long)seekTo);
            else SetPosition(0);
            //await LoadTransforming();
            return sessionId;
        }
    }
}
