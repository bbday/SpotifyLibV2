﻿using System.Net;

namespace SpotifyLibrary.Helpers.Msft
{
    internal static class TcpValidationHelpers
    {
        public static bool ValidatePortNumber(int port)
        {
            // When this method returns false, the caller should throw
            // 'new ArgumentOutOfRangeException("port")'
            return port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort;
        }
    }
}