namespace SpotifyLib.Models.Contexts
{
    public class SearchContext : GeneralFiniteContext
    {
        public readonly string SearchTerm;

        public SearchContext(
          string context,
          string searchTerm) : base(context)
        {
            SearchTerm = searchTerm;
        }
    }
}
