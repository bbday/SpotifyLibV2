using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Connectstate;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using Spotify.Player.Proto;
using Spotify.Player.Proto.Transfer;
using SpotifyLibrary.Connect.Contexts;
using SpotifyLibrary.Connect.Exceptions;
using SpotifyLibrary.Connect.Restrictions;
using SpotifyLibrary.Connect.TracksKeepers;
using SpotifyLibrary.Exceptions;
using SpotifyLibrary.Helpers;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Services.Mercury;
using SpotifyLibrary.Services.Mercury.Interfaces;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Models;
using ContextPlayerOptions = Connectstate.ContextPlayerOptions;

namespace SpotifyLibrary.Connect
{
    public static class SpotifyIdExtensions
    {
        public static bool Matches(this ISpotifyId id, ContextTrack track)
        {
            if (track.HasUri) return id.Uri.Equals(track.Uri);
            else if (track.HasGid)
                return Arrays.AreEqual(track.Gid.ToByteArray(), Utils.HexToBytes(id.ToHexId()));
            else return false;
        }
    }
    internal class LocalStateWrapper
    {
        private TracksKeeper _tracksKeeper;
        public readonly PlayerState PlayerState;
        private readonly SpotifyRequestState requestState;
        private HttpClient _cuePointsClient;

        internal LocalStateWrapper(SpotifyRequestState requestState)
        {
            this.requestState = requestState;
            PlayerState = InitState();
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
                _tracksKeeper.InitializeFrom(list =>
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
                Debug.WriteLine("Failed initializing tracks, falling back to start. uid: {}", ps.CurrentUid);
                _tracksKeeper.InitializeStart();
            }

            PlayerState.PositionAsOfTimestamp = pb.PositionAsOfTimestamp;

            if (pb.IsPaused) PlayerState.Timestamp = TimeProvider.CurrentTimeMillis();
            else PlayerState.Timestamp = pb.Timestamp;

            await LoadTransforming();
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

            Pages = PagesLoader.From(requestState.ConnectClient.Client.MercuryClient, ctx);
            _tracksKeeper = new TracksKeeper(this, PlayerState, Context);


            requestState.SetIsActive(true);

            return RenewSessionId();
        }
        private async Task LoadTransforming()
        {
            if (_tracksKeeper == null) throw new IllegalStateException();

            var url = PlayerState.ContextMetadata.GetMetadataOrDefault("transforming.url", null);
            if (url == null) return;

            var shuffle = false;
            if (PlayerState.ContextMetadata.ContainsKey("transforming.shuffle"))
                shuffle = bool.Parse(PlayerState.ContextMetadata["transforming.shuffle"]);

            var willRequest = !PlayerState.Track.Metadata.ContainsKey("audio.fwdbtn.fade_overlap");
            Debug.WriteLine($"Context has transforming: {url}, shuffle: {shuffle}, willRequest: {willRequest}");
            if (!willRequest) return;
            var obj = ProtoUtils.CraftContextStateCombo(PlayerState,
                _tracksKeeper.Tracks);
            try
            {
                _cuePointsClient ??= new HttpClient();
                var content = new StringContent(obj.ToString(), Encoding.UTF8, "application/json");
                _cuePointsClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer",  (await requestState.ConnectClient.Client.Tokens.GetToken("playlist-read"))
                        .AccessToken);
                var resp = await _cuePointsClient.PostAsync(url, content);
                if (resp != null && resp.IsSuccessStatusCode)
                {
                    var dt = await resp.Content.ReadAsStringAsync();
                    UpdateContext(JObject.Parse(dt));
                }
                Debug.WriteLine("Updated context with transforming information!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed loading cuepoints " +
                                $"Error returned from {ex.Message}");
                return;
            }
        }
        private void UpdateContext(JObject parse)
        {
            var uri = parse["uri"].ToString();
            if (!Context.Uri.Equals(uri))
            {
                Debug.WriteLine("Received update for the wrong context! context: {0}, newUri: {1}",
                    Context, uri);
                return;
            }

            ProtoUtils.CopyOverMetadata(((JObject)parse["metadata"])!, PlayerState);
            _tracksKeeper.UpdateContext(ProtoUtils.JsonToContextPages(((JArray)parse["pages"])!));
        }
        public void SetRepeatingContext(bool value)
        {
            if (Context == null) return;

            PlayerState.Options.RepeatingContext = true && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_CONTEXT);
        }

        public void SetRepeatingTrack(bool value)
        {
            if (Context == null) return;
            PlayerState.Options.RepeatingTrack = true && Context.Restrictions.Can(RestrictionsManager.Action.REPEAT_TRACK);
        }
        public void SetShufflingContext(bool value)
        {
            if (Context == null
                || _tracksKeeper == null) return;

            var old = IsShufflingContext;
            PlayerState.Options.ShufflingContext = value && Context.Restrictions.Can(RestrictionsManager.Action.SHUFFLE);

            if (old != IsShufflingContext)
                _tracksKeeper.ToggleShuffle(IsShufflingContext);

        }

        public ISpotifyId GetPlayableItem => _tracksKeeper == null
            ? null
            : PlayableId.From(PlayerState.Track);

        public int Position => (int) GetPosition();
        public PagesLoader Pages { get; private set; }

        public bool IsPaused => PlayerState.IsPlaying && PlayerState.IsPaused;
        public bool IsShufflingContext => PlayerState.Options.ShufflingContext;
        public bool IsRepeatingContext => PlayerState.Options.RepeatingContext;
        public bool IsRepeatingTrack => PlayerState.Options.RepeatingTrack;
        public string SessionId => PlayerState.SessionId;
        public string ContextUrl => PlayerState.ContextUrl;

        public uint ContextSize
        {
            get
            {
                var trackCount = PlayerState.ContextMetadata.GetMetadataOrDefault("track_count", null);
                if (!int.TryParse(trackCount, out var trackCountInt))
                {
                    if (_tracksKeeper != null)
                    {
                        return (uint) _tracksKeeper.Tracks.Count;
                    }
                    else
                    {
                        trackCountInt = 0;
                    }
                }

                return (uint) trackCountInt;
            }
        }

        private static PlayerState InitState()
        {
            return new PlayerState
            {
                PlaybackSpeed = 1.0,
                SessionId = string.Empty,
                PlaybackId = string.Empty,
                Suppressions = new Suppressions(),
                ContextRestrictions = new Connectstate.Restrictions(),
                Options = new ContextPlayerOptions
                {
                    RepeatingTrack = false,
                    ShufflingContext = false,
                    RepeatingContext = false
                },
                Position = 0,
                PositionAsOfTimestamp = 0,
                IsPlaying = false
            };
        }

        public int GetPosition()
        {
            int diff = (int) (TimeProvider.CurrentTimeMillis() - PlayerState.Timestamp);
            return (int) (PlayerState.PositionAsOfTimestamp + diff);
        }

        public void SetPlaybackId(string playbackId) => PlayerState.PlaybackId = playbackId;

        public void SetState(bool playing, bool paused, bool buffering)
        {
            if (paused && !playing) throw new IllegalStateException();
            else if (buffering && !playing) throw new IllegalStateException();

            var wasPaused = IsPaused;
            PlayerState.IsPlaying = playing;
            PlayerState.IsPaused = paused;
            PlayerState.IsBuffering = buffering;

            if (wasPaused && !paused) // Assume the position was set immediately before pausing
                SetPosition(PlayerState.PositionAsOfTimestamp);
        }
        public void SetPosition(long pos)
        {
            PlayerState.Timestamp = TimeProvider.CurrentTimeMillis();
            PlayerState.PositionAsOfTimestamp = pos;
            PlayerState.Position = 0L;
        }
        public AbsSpotifyContext Context { get; private set; }

        public async Task Updated(bool push = false)
        {
            UpdateRestrictions();
            // if (push)
            await requestState.UpdateState(PutStateReason.PlayerStateChanged,
                (int)requestState.Player.Position.TotalMilliseconds);
        }
        public void UpdateRestrictions()
        {
            if (Context == null) return;

            if (_tracksKeeper.IsPlayingFirst() && !IsRepeatingContext)
                Context.Restrictions.Disallow(RestrictionsManager.Action.SKIP_PREV, RestrictionsManager.REASON_NO_PREV_TRACK);
            else
                Context.Restrictions.Allow(RestrictionsManager.Action.SKIP_PREV);

            if (_tracksKeeper.IsPlayingLast() && !IsRepeatingContext)
                Context.Restrictions.Disallow(RestrictionsManager.Action.SKIP_NEXT, RestrictionsManager.REASON_NO_NEXT_TRACK);
            else
                Context.Restrictions.Allow(RestrictionsManager.Action.SKIP_NEXT);

            PlayerState.Restrictions = Context.Restrictions.ToProto();
            PlayerState.ContextRestrictions = Context.Restrictions.ToProto();
        }
        private string RenewSessionId()
        {
            var sessionId = GenerateSessionId();
            PlayerState.SessionId = sessionId;
            return sessionId;
        }
        private static string GenerateSessionId()
        {
            var bytes = new byte[16];
            (new Random()).NextBytes(bytes);
            var str = Base64UrlEncode(bytes);
            return str;
            //var str = Convert.ToBase64String(bytes).Replace("-", "");
            //str = str.Replace("=", "");
            //return str;
        }
 

        public static string GeneratePlaybackId()
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

        public void EnrichWithMetadata(TrackOrEpisode metadata)
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

        public void SetBuffering(bool b)
        {
            SetState(true, PlayerState.IsPaused, b);
        }

        internal Dictionary<string, string> MetadataFor(ISpotifyId playable)
        {
            if (_tracksKeeper == null) return new Dictionary<string, string>();

            var current = GetCurrentTrack();

            if (current != null && playable.Matches(current))
                return current.Metadata.ToDictionary(pair => pair.Key,
                    pair => pair.Value);

            var index = _tracksKeeper.Tracks.FindIndex(z =>
                (z.HasUri && playable.Uri.Equals(z.Uri)) ||
                (z.HasGid && z.Gid.Equals(ByteString.CopyFrom(Utils.HexToBytes(playable.ToHexId())))));

            if (index == -1)
            {
                index = _tracksKeeper.Queue.ToList().FindIndex(z =>
                    (z.HasUri && playable.Uri.Equals(z.Uri)) ||
                    (z.HasGid && z.Gid.Equals(ByteString.CopyFrom(Utils.HexToBytes(playable.ToHexId())))));
                if (index == -1) return new Dictionary<string, string>();
            }

            return _tracksKeeper.Tracks[index].Metadata.ToDictionary(pair => pair.Key,
                pair => pair.Value);
        }
        public ContextTrack GetCurrentTrack()
        {
            var index = PlayerState.Index.Track;
            return _tracksKeeper == null || _tracksKeeper.Tracks.Count < index ? null : _tracksKeeper.Tracks[(int)index];
        }
    }
}
