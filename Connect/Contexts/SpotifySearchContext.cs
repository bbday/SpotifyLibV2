using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Connect.Contexts
{
    public class SpotifySearchContext : GeneralFiniteContext
    {
        public SpotifySearchContext(string context, string search) : base(context)
        {
            SearchItem = search;
        }

        public string SearchItem { get; }
    }
}
