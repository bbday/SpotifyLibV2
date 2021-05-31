using System;
using System.Diagnostics;

namespace Spotify.Lib.Exceptions
{
    public class MercuryException : Exception
    {
        public MercuryException(string message) : base(message)
        {

        }
    }
    public class MercuryCannotReceiveException : MercuryException
    {
        public MercuryCannotReceiveException(string s) : base(s)
        {
            Debug.WriteLine(s);
        }
    }

    public class MercuryCannotSendException : MercuryException
    {
        public MercuryCannotSendException(string s) : base(s)
        {
        }
    }
}