namespace SpotifyLibV2.Models.Request
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