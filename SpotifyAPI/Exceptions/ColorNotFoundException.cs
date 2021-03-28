using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLibrary.Models.Response.Pathfinder;

namespace SpotifyLibrary.Exceptions
{
    public class ColorNotFoundException : Exception
    {
        public ColorNotFoundException(List<Error> errors,
            Uri url) : base($"Color not found for url: {url}")
        {
            Url = url;
            Errors = errors;
        }
        public List<Error> Errors { get; }
        public Uri Url { get; }
    }
}