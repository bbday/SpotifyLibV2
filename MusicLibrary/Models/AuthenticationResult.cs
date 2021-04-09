using MusicLibrary.Enum;

namespace MusicLibrary.Models
{
    public abstract class AuthenticationResult
    {
        protected AuthenticationResult(bool success, 
            string message, AudioService audioService)
        {
            Success = success;
            Message = message;
            AudioService = audioService;
        }

        public bool Success { get; }
        public string Message { get; }
        public AudioService AudioService { get; }
    }
}
