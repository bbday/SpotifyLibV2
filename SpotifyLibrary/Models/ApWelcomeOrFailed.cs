using System;
using Spotify;

namespace SpotifyLibrary.Models
{
    public readonly struct ApWelcomeOrFailed
    {
        internal ApWelcomeOrFailed(ApWelcomeOrFailed other,
            TimeSpan duration, Exception? exception)
        {
            Welcome = other.Welcome;
            Failed = other.Failed;
            Duration = duration;
            Exception = exception;
            Success = other.Success;
            Message = other.Message;
        }
        public ApWelcomeOrFailed(
            APWelcome? welcome,
            APLoginFailed? failed, TimeSpan duration, Exception? exception)
        {
            Welcome = welcome;
            Failed = failed;
            Duration = duration;
            Exception = exception;

            Success = welcome != null;
            Message = failed?.ErrorCode.ToString() ?? "";
        }

        public bool HasWelcome => Welcome != null;
        public bool HasFailed => Failed != null;

        public APWelcome? Welcome { get; }
        public APLoginFailed? Failed { get; }
        public Exception? Exception { get; }
        public TimeSpan Duration { get; }

        public bool Success { get; }
        public string? Message { get; }

    }
}
