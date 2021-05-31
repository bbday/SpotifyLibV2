

namespace Spotify.Lib.Helpers
{
    //public static class SpotifyPlaybackHelper
    //{
    //    //TODO: Indicate context
    //    public static AbsChunkedStream GetUrlStream(
    //        Track track, CdnUrl url,
    //        byte[] audioKey)
    //    {
    //        return new UrlStream(url, audioKey, new TrackOrEpisode(track));
    //    }

    //    public static AbsChunkedStream GetFileStream(
    //        Track track, string path,
    //        byte[] audioKey)
    //    {
    //        return new FileStreamInternal(path, audioKey, new TrackOrEpisode(track));
    //    }

    //    public static AbsChunkedStream GetByteStream(
    //        Track track, byte[] trackData,
    //        byte[] audioKey)
    //    {
    //        return new ByteStreamInternal(trackData, audioKey, new TrackOrEpisode(track));
    //    }

    //    public static Task<AbsChunkedStream> PlayUrl(
    //        Track track, CdnUrl url,
    //        PagedRequest context,
    //        byte[] audioKey)
    //    {
    //        var newStream = GetUrlStream(track, url, audioKey);
    //        return
    //            SpotifyConnectReceiver.Instance
    //                .RequestState
    //                .HandlePlayInternal(
    //                    track.Duration,
    //                    newStream, context, audioKey);
    //    }

    //    public static Task<AbsChunkedStream> PlayByteStr(
    //        Track track,
    //        byte[] str,
    //        PagedRequest context,
    //        byte[] audioKey)
    //    {
    //        var newStream = GetByteStream(track, str, audioKey);
    //        return
    //            SpotifyConnectReceiver.Instance
    //                .RequestState
    //                .HandlePlayInternal(
    //                    track.Duration,
    //                    newStream, context, audioKey);
    //    }

    //    public static Task<AbsChunkedStream> PlayFile(
    //        Track track,
    //        string path,
    //        PagedRequest context,
    //        byte[] audioKey)
    //    {
    //        var newStream = GetFileStream(track, path, audioKey);
    //        return
    //            SpotifyConnectReceiver.Instance
    //                .RequestState
    //                .HandlePlayInternal(
    //                    track.Duration,
    //                    newStream, context, audioKey);
    //    }
    //}
}