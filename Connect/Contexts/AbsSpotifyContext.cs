using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Spotify;
using SpotifyLibV2.Connect.Restrictions;
using SpotifyLibV2.Interfaces;

namespace SpotifyLibV2.Connect.Contexts
{
    public class UnsupportedContextException : Exception
    {
        public UnsupportedContextException(string message) : base(message)
        {

        }
        public static UnsupportedContextException CannotPlayAnything()
        {
            return new("Nothing from this context can or should be played!");
        }
    }

    public abstract class AbsSpotifyContext
    {
        public readonly RestrictionsManager Restrictions;
        protected readonly string Context;

        protected AbsSpotifyContext(string context)
        {
            Context = context;
            Restrictions = new RestrictionsManager(this);
        }

        public static bool IsCollection([NotNull] APWelcome apWelcome, [NotNull] string uri) =>
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
