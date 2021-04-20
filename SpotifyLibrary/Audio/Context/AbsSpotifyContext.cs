using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spotify;
using SpotifyLibrary.Audio.Restrictions;

namespace SpotifyLibrary.Audio.Context
{
    public abstract class AbsSpotifyContext
    {
        public readonly RestrictionsManager Restrictions;
        protected readonly string Context;

        protected AbsSpotifyContext(string context)
        {
            Context = context;
            Restrictions = new RestrictionsManager(this);
        }

        public static bool IsCollection(APWelcome apWelcome, string uri) =>
            string.Equals(uri, $"spotify:user:{apWelcome.CanonicalUsername}:collection");

        public static AbsSpotifyContext From(string context)
        {
            if (context.StartsWith("spotify:dailymix:") || context.StartsWith("spotify:station:"))
            {
                return new GeneralInfiniteContext(context);
            }

            return context.StartsWith("spotify:search")
                ? new SpotifySearchContext(context, context.Split(':').Last())
                : new GeneralFiniteContext(context);
        }

        public override string ToString() => $"AbsSpotifyContext : context = {Context}";
        public abstract bool IsFinite();
        public string Uri => Context;
    }
}