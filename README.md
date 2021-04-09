# SpotifyLib

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

## Features

- Full accesss to all spotify api endpoints. Both REST & Mercury
- Connect client 
- Play audio
- Cross platform (.NET Standard)


## Examples

### Authentication & Bearer

All REST Clients and other clients are LazyLoaded, which means they are not fetched/initialized until the developer specifically requests them. 

You can create a new instance of the class ```SpotifyClient```.
The parameter ```ICacheManager``` is **optional**. (The developer has to implement this themselves.)

The ```SpotifyClient.Authenticate``` function accepts a parameter of type: ```SpotifyConfiguration```
Which accepts the following paramemters

- Type: IAuthenticator:
-- Out of the box there are 2 implementations you can directly use. For userpass use ```UserPassAuthenticator```

```
var newSpotifyClient = new SpotifyClient();

var userDataAuthenticator = new UserPassAuthenticator(USERNAME, PASSWORD);
var newConfiguration = new SpotifyConfiguration(userDataAuthenticator, DEVICENAME);
var authenticationResultTask = newSpotifyClient.Authenticate(newConfiguration, false);
```

```SpotifyClient.Authenticate``` returns ```Task<SpotifyConnectionResult>``` which contains the fields :

- Success : **bool** Boolean indicating if authentication was successfull.
- Message : **string**  Message if authentication failed.
- ApWelcome : **APWelcome** ONLY FILLED IF AUTHENTICATION WAS SUCCESSFULL
- ApFailed : **APFailed** ONLY FILLED IF AUTHENTICATION FAILED

Once authentication was successfull, you can proceed to generate a bearer token using:
```SpotifyClient.Tokens.GetToken()```

```
var tokensClient = newSpotifyClient.Tokens;
var myBearerKey = tokensClient.GetToken("playlist-read")
```

All bearer keys are valid for 1 hour and are reused. 

### Be careful!

```GetToken()``` returns an instance of type ```StoredToken``` which has the field : ```AccessToken```.
```StoredToken``` also implements ToString() so it may look like it is the actual bearer, but it is not.
