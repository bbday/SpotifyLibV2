using System;
using System.Collections.Generic;
using System.Text;
using SpotifyLib.Helpers;

namespace SpotifyLib.Models.Contexts
{
    public abstract class AbsSpotifyContext
    {
        public readonly RestrictionsManager Restrictions;
        public readonly string Context;

        internal AbsSpotifyContext(string context)
        {
            this.Context = context;
            this.Restrictions = new RestrictionsManager(this);
        }
        public static AbsSpotifyContext From(string context)
        {
            if (context.StartsWith("spotify:dailymix:") || context.StartsWith("spotify:station:"))
                return new GeneralInfiniteContext(context);
            else if (context.StartsWith("spotify:search:"))
                return new SearchContext(context, context.Substring(15));
            else
                return new GeneralFiniteContext(context);
        }
        public abstract bool IsFinite { get; }
        public string Uri => Context;

        public override string ToString() => "AbsSpotifyContext{context='" + Context + "'}";
    }
}
