using System;

namespace SpotifyLibV2.Exceptions
{
    public class PubSubException : Exception
    {
        public PubSubException(int message) : base(message.ToString())
        {

        }
    }
}