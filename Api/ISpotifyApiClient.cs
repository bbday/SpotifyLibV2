﻿using Nito.AsyncEx;
using SpotifyLib.Api;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Api
{
    public interface ISpotifyApiClient
    {
        AsyncLazy<IHomeClient> Home { get; }
        ITokensProvider Tokens { get; }
        IEventsService EventsService { get; }
        IMercuryClient MercuryClient { get; }
        AsyncLazy<IStorageResolveService> StorageResolve { get; }
        AsyncLazy<IPlaybackLicense> LicenseService { get; }
        AsyncLazy<ISeektables> SeekTables { get; }
        AsyncLazy<IPathFinder> PathFinder { get; }
        AsyncLazy<IPlayerClient> PlayerClient { get; }
        AsyncLazy<IConnectState> ConnectApi { get; }
        AsyncLazy<ITrack> Tracks { get; }
        AsyncLazy<IEpisodes> Episodes { get; }
        AsyncLazy<ILibrary> Library { get; }
        AsyncLazy<IAlbum> Album { get; }
        AsyncLazy<IArtist> Artist { get; }
        AsyncLazy<IPlaylist> Playlist { get; }
        AsyncLazy<IUserService> User { get; }
        AsyncLazy<IMetadata> Metadata { get; }
        AsyncLazy<IMeClient> Me { get; }

    }
}
