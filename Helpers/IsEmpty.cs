using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Helpers
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string input) => string.IsNullOrEmpty(input);
    }
}
