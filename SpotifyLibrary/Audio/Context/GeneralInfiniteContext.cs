namespace SpotifyLibrary.Audio.Context
{
    public class GeneralInfiniteContext : AbsSpotifyContext
    {
        public GeneralInfiniteContext(string context) : base(context)
        {
        }

        public override bool IsFinite() => false;
    }
}
