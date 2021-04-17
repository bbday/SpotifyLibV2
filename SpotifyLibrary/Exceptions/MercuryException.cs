using System;
using System.Diagnostics;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Exceptions
{
    public class MercuryException : Exception
    {
        public MercuryException(MercuryResponse response) : base(response.StatusCode.ToString())
        {
            Debug.WriteLine("Mercury failed: Response " + response.StatusCode);
        }
    }
}