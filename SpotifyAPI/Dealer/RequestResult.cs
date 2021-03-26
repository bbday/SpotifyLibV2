namespace SpotifyLibrary.Dealer
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