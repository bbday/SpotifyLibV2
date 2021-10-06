using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spotify.Download.Proto;
using Spotify.Metadata.Proto;
using SpotifyLib.Helpers;
using SpotifyLib.Models.Transitions;

namespace SpotifyLib.Models.Player
{
    public readonly struct MetadataWrapper
    {

    }
    public class PlayerSession : IDisposable
    {
        private readonly IAudioOutput _player;
        private readonly string _sessionId;
        private readonly PlayerQueue _queue;
        private readonly SpotifyWebsocketState _connState;
        public PlayerSession(SpotifyWebsocketState connState, 
            string sessionId)
        {
            _connState = connState;
            _player = _connState.AudioOutput;
            _sessionId = sessionId;
            _queue = new PlayerQueue();
        }

        public event EventHandler<ChunkedStream> FinishedLoading;

        private int LastPlayPos = 0;
        private TransitionReason? LastPlayReason = null;

        public async Task<ChunkedStream> Load(SpotifyId stateWrapperGetPlayableItem,
            int stateWrapperPosition,
            TransitionReason transitionInfoStartedReason)
        {
            return (await LoadInternal(stateWrapperGetPlayableItem,
                stateWrapperPosition, transitionInfoStartedReason)).Entry.Item;
        }

        private async Task<(QueueNode<ChunkedStream> Entry, int Position)>
            LoadInternal(
            SpotifyId stateWrapperGetPlayableItem,
            int stateWrapperPosition, TransitionReason transitionInfoStartedReason)
        {
            LastPlayPos = (int)stateWrapperPosition;
            LastPlayReason = transitionInfoStartedReason;

            if (!AdvanceTo(stateWrapperGetPlayableItem))
            {
                await Add(stateWrapperGetPlayableItem);
                PlayerQueueHelper.Advance(_queue);
            }
            var head = _queue.Head;
            if (head == null)
                throw new IllegalStateException();

            Debug.WriteLine($"{head.Item} has been added to output.");
            return (head, LastPlayPos);
        }
        private async Task Add(SpotifyId playable,
            CancellationToken ct = default)
        {
            //TODO: fetch stream..
            var cachedStream = await
                                   _player.GetCachedStream(playable)
                               ?? await GetCdnStream(playable, _connState.ConState, ct);
            FinishedLoading?.Invoke(this, cachedStream);
            PlayerQueueHelper.Add(cachedStream, _player, _queue);
        }

        private static async Task<ChunkedStream> GetCdnStream(SpotifyId id,
            SpotifyConnectionState connState,
            CancellationToken ct)
        {
            var (track, file) = await GetFile(id, connState,
                ct);
            //figure out preload.
            var resp =
                await connState.ResolveStorageInteractive(file.FileId, false, ct);
            switch (resp.Result)
            {
                case StorageResolveResponse.Types.Result.Cdn:
                    var audioKey = await connState.GetAudioKey(track.Gid,
                        resp.Fileid, ct: ct);
                    var cdnurl = new CdnUrl(file.FileId, new Uri(resp.Cdnurl.First()));

                    var chunkResponse = await cdnurl
                        .GetRequest(connState, 0,
                            Consts.CHUNK_SIZE - 1);
                    var contentRange = chunkResponse.Headers["Content-Range"];
                    if (string.IsNullOrEmpty(contentRange))
                        throw new Exception("Missing Content-Range header");

                    var split = contentRange.Split('/');
                    var size = int.Parse(split[1]);
                    var chunks = (int)Math.Ceiling((float)size / (float)Consts.CHUNK_SIZE);

                    return new ChunkedStream(id, cdnurl, track, audioKey, size, chunks, chunkResponse.Stream,
                        connState);
                default:
                    //TODO: Figure out what this is?
                    Debugger.Break();
                    break;
            }

            return null;
        }

        private bool AdvanceTo(SpotifyId id)
        {
            do
            {
                var entry = _queue.Head;
                if (entry == null) return false;
                if (entry.Item.Id.Equals(id))
                {
                    var next = _queue.Next;
                    if (next == null || !next.Item.Id.Equals(id))
                    {
                        return true;
                    }
                }
            } while (PlayerQueueHelper.Advance(_queue));

            return false;
        }

        public static async Task<(Track Track, AudioFile File)> GetFile(SpotifyId spotifyId,
            SpotifyConnectionState connState,
            CancellationToken ct = default)
        {
            var original = await spotifyId.FetchAsync<Track>(connState, ct);
            
            var track = PickAlternativeIfNecessary(original);

            if (track == null)
            {
                var country = connState.Country;
                if (country != null)
                    ContentRestrictedException.CheckRestrictions(country, original.Restriction.ToList());

                Debug.WriteLine("Couldn't find playable track: " + spotifyId.ToString());
                throw new FeederException();
            }

            var file = VorbisOnlyAudioQuality.GetFile(track.File.ToList(), AudioQuality.VERY_HIGH);
            if (file == null)
            {
                Debug.WriteLine("Couldn't find any suitable audio file, available: " + string.Join(Environment.NewLine,
                    track.File.ToList().Select(z => z.FileId.ToBase64())));
                throw new FeederException();
            }

            return (track, file);
        }
        private static Track PickAlternativeIfNecessary(Track track)
        {
            if (track.File.Count > 0) return track;

            return track.Alternative.FirstOrDefault(z => z.File.Count > 0);
        }


        public void Dispose()
        {
            _queue.Head?.Dispose();
            _queue.Next?.Dispose();
        }
    }
}
