using System;
using System.Collections.Generic;
using System.Text;

namespace SpotifyLibV2.Connect.Contexts
{
    public class GeneralFiniteContext : AbsSpotifyContext
    {
        public GeneralFiniteContext(string context) : base(context)
        {
            //
        }

        public override bool IsFinite() => true;
    }
}
