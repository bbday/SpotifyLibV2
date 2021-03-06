# SpotifyLib

Working on bringing back audio/better documentation, sorry!

A real simple (Yet functional) program can be created using the following snippet if you wanna mess around:

```
   var mn = new ManualResetEvent(false);
            var connState = await SpotifyClientMethods.Authenticate(
                new UserpassAuthenticator("SPOTIFY EMAIL", "SPOTIFY PASSWORD"), SpotifyConfig.Default(),
                CancellationToken.None);

            var websocket = await SpotifyWebsocketState
                .ConnectToRemote(connState);
            Console.WriteLine($"Connected to ws with initial cluster: {websocket.LatestCluster.PlayerState.Track.Uri} " +
                              $"on Device: {websocket.ActiveDevice.Name}");
            websocket.ClusterUpdated += (sender, update) =>
            {
                Console.WriteLine($"New cluster: {update.PlayerState.Track?.Uri}");
            };
            websocket.ActiveDeviceChanged += (sender, tuple) =>
            {
                var (old, spotifyDevice) = tuple;
                Console.WriteLine($"Device switched from {old?.Name} to {spotifyDevice?.Name}");
            };

            mn.WaitOne();
```

## ABOUT
a .NET Standard implementation of spotify.

Implements a Mercury client to communicate with the hm:// endpoints used by the spotify desktop client.
Also contains normal Rest endpoints to communicate with https endpoints.

hm is short hermes, a protocol used internally between servers at Spotify. It is basically zeromq with a protobuf envelope with some defined headers.

So, kind of like HTTP define verbs and structure on-top of TCP, Hermes define verbs and structure on-top of zeromq. It is used for HTTP-like Request/Response as well as Publish/Subscribe. For instance, a client request data about an album and waits for a response. Another example could be a client subscribing to events about a playlist. The moment someone publishes a change to the playlist, the client will know.

In one sense, it can do more than HTTP, but in another sense, it is much simpler because of the limited use. It was built many years ago, before HTTP/2 and grpc. It is still used heavily at Spotify.
(https://www.csc.kth.se/~gkreitz/spotifypubsub/spotifypubsub.pdf)


Spotify has been slowly moving away from hermes in exchange for normal rest & wss:// (websockets). 
Nevertheless, there are still a lot calls that are not fully supported by rest (For example the social feed, see  Task AttachSocial() in SpotifySession.cs).
