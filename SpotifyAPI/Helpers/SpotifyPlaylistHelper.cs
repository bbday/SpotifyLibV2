using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using JetBrains.Annotations;
using MusicLibrary.Enum;
using MusicLibrary.Interfaces;
using Newtonsoft.Json;
using Spotify.Playlist4.Proto;
using SpotifyLibrary.Attributes;
using SpotifyLibrary.ClientHandlers;
using SpotifyLibrary.Enum;
using SpotifyLibrary.Helpers.Extensions;
using SpotifyLibrary.Models.Ids;
using SpotifyLibrary.Models.Playlists;
using SpotifyLibrary.Models.Request;
using SpotifyLibrary.Models.Response.SpotifyItems;
using SpotifyLibrary.Sql;
using SpotifyLibrary.Sql.DBModels;

namespace SpotifyLibrary.Helpers
{
    public struct PlistTrackQuick
    {
        
        public PlistTrackQuick(string uri,
            DateTime addedAt,
            [CanBeNull] string addedBy)
        {
            Uri = uri;
            AddedAt = addedAt;
            AddedBy = addedBy;
            Id = uri.Split(':').Last();
        }

        public string Id { get; set; }
        public string Uri { get; set; }
        public DateTime AddedAt { get; set; }
        public string AddedBy { get; set; }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }
    }

    public class SpotifyPlaylistHelper
    {
        private readonly SpotifyClient _client;
        private HttpClient __c;

        private HttpClient _metadataClient
        {
            get
            {
                if (__c != null) return __c;
                var handler = new LoggingHandler(new HttpClientHandler(),
                    message => Debug.WriteLine($"New metadata request"), _client.Tokens);

                __c = new HttpClient(handler)
                {
                    BaseAddress = new
                        Uri(ApResolver.GetClosestSpClient().ConfigureAwait(false)
                            .GetAwaiter().GetResult())
                };
                _metadataClient.DefaultRequestHeaders.Add("Accept", "application/x-protobuf");
                return __c;
            }
        }

        public SpotifyPlaylistHelper(SpotifyClient client)
        {
            _client = client;
        }

        public Task<List<IPlaylistTrack>>
            FetchTracks(SelectedListContent selectedListcontent,
                PlaylistId id)
        {
            var plitType =
                ParsePlaylistType(selectedListcontent);

            var plistTrackQuicks = selectedListcontent.Contents
                .Items
                .Select(z => new PlistTrackQuick(z.Uri, DateTime.UtcNow, null))
                .ToList();
            return FetchTracks(plistTrackQuicks, plitType);
        }

        public async Task<List<IPlaylistTrack>>
            FetchTracks(List<PlistTrackQuick> plistTrackQuicks, PlaylistType plitType)
        {
            var bag = new ConcurrentBag<IEnumerable<IPlaylistTrack>>();
            var dictionary = new ConcurrentDictionary<string, int>();

            var tsks = new List<Task>();

            using var sqlCon = SqlDb.Connection(_client.SqlPath);
            var items =
                SqlDb.GetTracks(sqlCon, plistTrackQuicks
                    .Select(z => z.Id));

            var toFetchTracks = plistTrackQuicks
                .Where(z => !z.Uri.StartsWith("spotify:local"))
                .Select(z => z.Id)
                .Except(items.Select(z => z.Id))
                .ToList();

            var availableTracks =
                plistTrackQuicks.Where(z =>
                    toFetchTracks.All(a => a != z.Id));

            tsks.AddRange(availableTracks
                .GroupBy(z => z.Uri)
                .Select(z => Task.Run(() =>
                {
                    var uriCount = 0;
                    foreach (var itemInGroup in z)
                    {
                        IPlaylistTrack trackToAdd = default(IPlaylistTrack);
                        var uri = itemInGroup.Uri;

                        var getAllTracks = FindAllIndexof(plistTrackQuicks, uri);
                        var (index, plistTrackQuick) = getAllTracks[uriCount];
                        uriCount++;

                        if (uri.StartsWith("spotify:local"))
                        {
                            var replace = uri.Replace("spotify:local:",
                                "");
                            var decodedpart = Url.Decode(replace, true);
                            //Sou+x+いすぼくろ:SUNRISE:命ばっかり:252 (Artist :  ALBUM : NAME : SECONDS)
                            //火花::火花・話１:2776
                            var splitted = decodedpart.Split(':');

                            var artist = splitted[0];
                            var album = splitted[1];
                            var name = splitted[2];
                            if (!long.TryParse(splitted[3], out var ms))
                            {
                                ms = 0;
                            }

                            //TODO: try and read local path
                            //trackToAdd = new SpotifyLocalTrack(null,
                            //    name,
                            //    album,
                            //    artist,
                            //    TimeSpan.FromSeconds(ms),
                            //    index + 1,
                            //    plistTrackQuick.AddedAt,
                            //    plistTrackQuick.AddedBy);
                        }
                        else
                        {
                            var item = items
                                .FirstOrDefault(j => j.Id == itemInGroup.Id);
                            if (item == null)
                            {
                                trackToAdd = null;
                            }
                            else
                            {
                                switch (plitType)
                                {
                                    case PlaylistType.Collection:
                                    case PlaylistType.UserPlaylist:
                                    case PlaylistType.MadeForUser:
                                    case PlaylistType.Radio:
                                        trackToAdd =
                                            new SpotifyPlaylistTrack(item, index + 1,
                                                plistTrackQuick.AddedAt,
                                                plistTrackQuick.AddedBy,
                                                TrackType.PlaylistTrack, AudioService.Spotify);
                                        break;
                                    case PlaylistType.ChartedList:
                                        trackToAdd = new ChartTrack(item, index + 1,
                                            plistTrackQuick.AddedAt,
                                            plistTrackQuick.AddedBy,
                                            //itemInGroup.Operation ?? new ChartItemOperation(null),
                                            TrackType.ChartTrack);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }

                        if (trackToAdd != null)
                        {
                            bag.Add(new[] {trackToAdd});
                        }
                    }
                })));

            var batches = Batch(toFetchTracks, 50);

            tsks.AddRange(batches.Select(async item =>
            {
                var response = await (await _client.TracksClient).GetSeveral(
                    new TracksRequest(item.ToArray()));

                var dbTracks = response.Tracks.Select(FullTrackToDbTrack);
                var enumerable = dbTracks.ToList();
                SqlDb.CreateTracks(sqlCon, enumerable);
                bag.Add(enumerable
                    .Where(z=> z != null)
                    .Select((z, a) =>
                    {
                        try
                        {
                            if (!dictionary.TryGetValue(z.Uri, out var count))
                            {
                                count = 0;
                            }

                            var getAllTracks = FindAllIndexof(plistTrackQuicks, z.Uri);
                            var (index, plistTrackQuick) = getAllTracks[count];
                            dictionary.AddOrUpdate(z.Uri, count + 1);
                            switch (plitType)
                            {
                                case PlaylistType.Collection:
                                case PlaylistType.UserPlaylist:
                                case PlaylistType.MadeForUser:
                                case PlaylistType.Radio:
                                    return new SpotifyPlaylistTrack(z, index + 1,
                                        plistTrackQuick.AddedAt,
                                        plistTrackQuick.AddedBy, TrackType.PlaylistTrack, AudioService.Spotify
                                    );
                                case PlaylistType.ChartedList:
                                    return new SpotifyPlaylistTrack(z, index + 1,
                                        plistTrackQuick.AddedAt,
                                        plistTrackQuick.AddedBy, TrackType.ChartTrack, AudioService.Spotify);
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        catch (Exception x)
                        {
                            Debugger.Break();
                            throw;
                        }
                    }));
            }));

            await Task.WhenAll(tsks);

            return bag.SelectMany(z => z)
                .ToList();
        }

        public async Task<IDiffResult> DiffRootlist(string userId, RevisionId revision)
        {
            //we are gonna have to diff playlists and return an IEnumerable of hermmes playlist opsv
            var formattedUri =
                $"/playlist/v2/user/{userId}/rootlist/diff?revision={Url.Encode(revision.ToString())}&handlesContent=";
            var getByteData = await
                _metadataClient.GetByteArrayAsync(formattedUri);
            var data = SelectedListContent.Parser.ParseFrom(getByteData);

            var to = new RevisionId(data.Revision.ToBase64());

            if (data.Contents != null)
            {
                return new NewListDiff();
            }

            return new ItemsDiff(revision, to, !revision.Equals(to),
                data.Diff?.Ops.Select(z => new HermesPlaylistOperation(z))
                ?? new List<HermesPlaylistOperation>(0));
        }
        public async Task<IDiffResult> DiffPlaylist(PlaylistId id, RevisionId revision)
        {
            //we are gonna have to diff playlists and return an IEnumerable of hermmes playlist opsv
            var formattedUri =
                $"/playlist/v2/playlist/{id.Id}/diff?revision={Url.Encode(revision.ToString())}&handlesContent=";
            var getByteData = await
                _metadataClient.GetByteArrayAsync(formattedUri);
            var data = SelectedListContent.Parser.ParseFrom(getByteData);

            var to = new RevisionId(data.Revision.ToBase64());

            if (data.Contents != null)
            {
                return new NewListDiff();
            }

            return new ItemsDiff(revision, to, !revision.Equals(to),
                data.Diff?.Ops.Select(z => new HermesPlaylistOperation(z))
                ?? new List<HermesPlaylistOperation>(0));
        }
        public async Task<(IPlaylistHeader? headerItem, PlaylistType type)> GetPlaylistHeader(
            SelectedListContent item,
            FullPlaylist playlistdata)
        {
            IPlaylistHeader? headerItem = null;
            PlaylistType type;
            var folCount = playlistdata.Followers.Total;
            switch (item.Attributes.Format)
            {
                case "inspiredby-mix":
                    type = PlaylistType.UserPlaylist;
                    var inspiredByCaptions = new List<object>
                    {
                        "By",
                        new Creator(playlistdata.Owner.Uri, playlistdata.Owner.DisplayName),
                        item.Length + " tracks, {0}"
                    };
                    StringAttribute.GetValue(typeof(PlaylistType), type, out var topStringInspired);
                    headerItem = new PlaylistNormalHeader(playlistdata.Id, playlistdata.Name,
                        playlistdata.Description,
                        playlistdata.Images?.FirstOrDefault()?.Url,
                        topStringInspired, inspiredByCaptions,
                        folCount);
                    break;
                case "chart":
                    type = PlaylistType.ChartedList;
                    var lastUpdateDateTime = DateTime.Parse(item.Attributes.FormatAttributes
                        .First(z => z.Key == "last_updated")
                        .Value);
                    var captionItems = new List<object>
                    {
                        new BlueBubble(int.Parse(item.Attributes.FormatAttributes
                            .First(z => z.Key == "new_entries_count")
                            .Value)),
                        $"Latest update: {lastUpdateDateTime}"
                    };
                    StringAttribute.GetValue(typeof(PlaylistType), type, out var topStringCaption);
                    headerItem = new PlaylistNormalHeader(playlistdata.Id, playlistdata.Name,
                        playlistdata.Description,
                        playlistdata.Images?.FirstOrDefault()?.Url,
                        topStringCaption, captionItems,
                        folCount);
                    break;
                default:
                    var containtsMadeForuser =
                        item.Attributes.FormatAttributes.FirstOrDefault(z => z.Key == "madeFor.username");
                    if (containtsMadeForuser != null
                    && !item.Attributes.FormatAttributes.Any(z => z.Key == "header_image_url_desktop"))
                    {
                        type = PlaylistType.MadeForUser;
                        StringAttribute.GetValue(typeof(PlaylistType), type, out var dailyMixcaption);
                        //madeFor.username
                        var madeForuserName = containtsMadeForuser.Value;
                        var userKey = $"user-{madeForuserName}";
                        if (!_client.CacheManager.TryGetItem<string>(userKey, out var name))
                        {
                            var getUser = await (await _client.UserClient).GetUser(madeForuserName);
                            name = getUser.DisplayName;
                            _client.CacheManager.SaveItem(userKey, name);
                        }

                        var formatted = string.Format(dailyMixcaption!, name);


                        var captionItems2 = new List<object>
                        {
                            new DescriptoryPlaylist(new Creator($"spotify:user:{madeForuserName}", name)
                                , new Creator("spotify:user:spotify", "Spotify")),
                            item.Length + " tracks, {0}"
                        };

                        headerItem = new PlaylistNormalHeader(playlistdata.Id, playlistdata.Name,
                            playlistdata.Description,
                            playlistdata.Images?.FirstOrDefault()?.Url,
                            formatted, captionItems2, folCount);
                    }
                    else
                    {
                        type = PlaylistType.UserPlaylist;
                        StringAttribute.GetValue(typeof(PlaylistType), type, out var normal);
                        if (item.Attributes.FormatAttributes != null
                            && item.Attributes.FormatAttributes.Any(z => z.Key == "header_image_url_desktop"))
                        {
                            //Big headered.
                            var headerUrl =
                                item.Attributes.FormatAttributes.First(z => z.Key == "header_image_url_desktop")
                                    .Value;

                            var totalFormatted = $"{playlistdata.Followers.Total:#,##0}";
                            var bigCaptions = new List<object>
                            {
                                "By",
                                new Creator(playlistdata.Owner.Uri, playlistdata.Owner.DisplayName),
                                item.Length + " tracks, {0}"
                            };
                            headerItem = new PlaylistBigHeader(playlistdata.Id, playlistdata.Name,
                                playlistdata.Description,
                                playlistdata.Images?.FirstOrDefault()?.Url,
                                headerUrl,
                                normal, bigCaptions, folCount);
                        }
                        else
                        {
                            //Normal list.
                            var normalCaptions = new List<object>
                            {
                                "By",
                                new Creator(playlistdata.Owner.Uri, playlistdata.Owner.DisplayName),
                                item.Length + " tracks, {0}"
                            };
                            headerItem = new PlaylistNormalHeader(playlistdata.Id, playlistdata.Name,
                                playlistdata.Description,
                                playlistdata.Images?.FirstOrDefault()?.Url,
                                normal, normalCaptions, folCount);
                        }
                    }

                    break;
            }

            return (headerItem, type);
        }

        public async Task<SelectedListContent> GetSelectedListcontent(PlaylistId playlistId)
        {
            var mercuryListData = await
                (await _client.MetadataClient).GetPlaylistWithContent(playlistId.Id, "application/x-protobuf");
            var bytes = await
                mercuryListData.Content.ReadAsByteArrayAsync();
            var mercuryList = SelectedListContent.Parser.ParseFrom(bytes);
            return mercuryList;
        }

        private static IEnumerable<Memory<TSource>> Batch<TSource>(
            IEnumerable<TSource> source, int size)
        {
            TSource[]? bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                bucket ??= new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToArray();
        }
        private static (int Index, PlistTrackQuick Item)[] FindAllIndexof(IEnumerable<PlistTrackQuick> values,
            string uri)
        {
            return values.Select((x, i) => new { x, i })
                .Where(x => x.x.Uri == uri)
                .Select(z => (z.i, z.x))
                .ToArray();
        }
        private PlaylistType ParsePlaylistType(SelectedListContent content)
        {
            switch (content.Attributes.Format)
            {
                case "inspiredby-mix":
                    return PlaylistType.UserPlaylist;
                case "release-radar":
                case "daily-mix":
                    return PlaylistType.MadeForUser;
                default:
                    return PlaylistType.UserPlaylist;
            }
        }
        public static DbTrack FullTrackToDbTrack(FullTrack track)
        {
            if (track == null)
            {
                return null;
            }
            var idAsSpotify = track.Id as TrackId;
            var artistsString = JsonConvert.SerializeObject(track.Artists,
                Formatting.None,
                SqlDb.Settings);
            var groupString = JsonConvert.SerializeObject(track.Group,
                Formatting.None,
                SqlDb.Settings);
            return new DbTrack
            {
                ArtistsString = artistsString,
                GroupString = groupString,
                Id = idAsSpotify.Id,
                ImagesUrl = idAsSpotify.Uri,
                CanPlay = track.CanPlay,
                DurationMs = track.DurationMs,
                Service = AudioService.Spotify.ToString(),
                Type = track.Type.ToString(),
                Title = track.Name,
                Uri = track.Uri
            };
        }
        private static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

    }
}