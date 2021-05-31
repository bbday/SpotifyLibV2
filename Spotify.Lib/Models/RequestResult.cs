namespace Spotify.Lib.Models
{
    public enum RequestResult
    {
        UnknownSendCommandResult,
        Success,
        DeviceNotFound,
        ContextPlayerError,
        DeviceDisappeared,
        UpstreamError,
        DeviceDoesNotSupportCommand,
        RateLimited
    }
}