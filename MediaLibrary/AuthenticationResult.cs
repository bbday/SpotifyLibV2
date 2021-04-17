using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Enums;

namespace MediaLibrary
{ public abstract class AuthenticationResult
    {
        protected AuthenticationResult(bool success,
            string message, AudioServiceType audioService)
        {
            Success = success;
            Message = message;
            AudioService = audioService;
        }

        public bool Success { get; }
        public string Message { get; }
        public AudioServiceType AudioService { get; }
    }
}

