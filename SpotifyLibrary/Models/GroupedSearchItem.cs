using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MediaLibrary.Interfaces;

namespace SpotifyLibrary.Models
{
    public class GroupedSearchItem : List<IAudioItem>
    {
        public GroupedSearchItem(IEnumerable<IAudioItem> items,
            string key,
            string header, ICommand onClick) : base(items.Take(4))
        {
            Key = key;
            Header = header;
            OnClick = onClick;
        }

        public string Key { get; }
        public string Header { get;  }
        public ICommand OnClick { get; }
    }
}