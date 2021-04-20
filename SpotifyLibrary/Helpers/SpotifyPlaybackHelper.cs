using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SpotifyLibrary.Audio;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;
using SpotifyLibrary.Models.Requests;
using SpotifyProto;

namespace SpotifyLibrary.Helpers
{
    public static class SpotifyPlaybackHelper
    {
        //TODO: Indicate context
        public static AbsChunkedStream GetUrlStream(ISpotifyLibrary library,
            Track track, CdnUrl url,
            byte[] audioKey)
        {
            return new UrlStream(url, audioKey, new TrackOrEpisode(track));
        }
        public static AbsChunkedStream GetFileStream(ISpotifyLibrary library,
            Track track, string path,
            byte[] audioKey)
        {
            return new FileStreamInternal(path, audioKey, new TrackOrEpisode(track));
        }
        public static AbsChunkedStream GetByteStream(ISpotifyLibrary library,
            Track track, byte[] trackData,
            byte[] audioKey)
        {
            return new ByteStreamInternal(trackData, audioKey, new TrackOrEpisode(track));
        }
        public static Task<AbsChunkedStream> PlayUrl(ISpotifyLibrary library,
            Track track, CdnUrl url,
            PagedRequest context,
            byte[] audioKey)
        {
            var newStream = GetUrlStream(library, track, url, audioKey);
            return 
                (library as SpotifyLibrary)!
                .newConnectClient
                .RequestState
                .HandlePlayInternal(
                    track.Duration,
                    newStream, context, audioKey);
        }
        public static Task<AbsChunkedStream> PlayByteStr(ISpotifyLibrary library,
            Track track,
            byte[] str,
            PagedRequest context,
            byte[] audioKey)
        {
            var newStream = GetByteStream(library, track, str, audioKey);
            return
                (library as SpotifyLibrary)!
                .newConnectClient
                .RequestState
                .HandlePlayInternal(
                    track.Duration,
                    newStream, context, audioKey);
        }
        public static Task<AbsChunkedStream> PlayFile(ISpotifyLibrary library,
            Track track, 
            string path,
            PagedRequest context,
            byte[] audioKey)
        {
            var newStream = GetFileStream(library, track, path, audioKey);
            return
                (library as SpotifyLibrary)!
                .newConnectClient
                .RequestState
                .HandlePlayInternal(
                    track.Duration,
                    newStream, context, audioKey);
        }
    }
}
