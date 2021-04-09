using System;
using System.Collections.Generic;
using System.Text;

namespace MusicLibrary.Models
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

        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
