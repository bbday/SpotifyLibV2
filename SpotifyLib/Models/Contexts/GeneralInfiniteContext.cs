namespace SpotifyLib.Models.Contexts
{
    public class GeneralInfiniteContext : AbsSpotifyContext
    {
        internal GeneralInfiniteContext(string context) : base(context)
        {
        }

        public override bool IsFinite => false;
    }
}
