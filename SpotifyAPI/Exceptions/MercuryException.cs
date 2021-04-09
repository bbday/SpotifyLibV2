using System;
using System.Diagnostics;
using SpotifyLibrary.Models.Response.Mercury;

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