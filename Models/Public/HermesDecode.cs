using System;

namespace SpotifyLibV2.Models.Public
{
    internal class HermesDecode
    {
        internal HermesDecode(string hex, Lazy<string> utf)
        {
            Hex = hex;
            Utf = utf;
        }
        internal string Hex { get; }
        internal Lazy<string> Utf { get; }
    }
}
