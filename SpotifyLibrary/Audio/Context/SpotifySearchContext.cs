namespace SpotifyLibrary.Audio.Context
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
