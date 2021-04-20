namespace SpotifyLibrary.Audio.Context
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
