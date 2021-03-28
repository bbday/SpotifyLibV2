namespace SpotifyLibrary.Models.Response.SpotifyItems
{
    public class UrlImage
    {
        private string _mainUrl;
        public string Url
        {
            get => _mainUrl;
            set
            {
                if (value != null)
                    _mainUrl = value;
            }
        }

        public string Uri
        {
            get => _mainUrl;
            set
            {
                if (value != null)
                    _mainUrl = value;
            }
        }
    }
}
