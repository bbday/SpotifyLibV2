using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Player.Proto;
using SpotifyLibV2.Exceptions;
using SpotifyLibV2.Helpers;
using SpotifyLibV2.Ids;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Connect.Contexts
{
    public class PagesLoader
    {
        private readonly List<ContextPage> _pages;
        private readonly IMercuryClient _mercuryClient;

        public string ResolveUrl
        {
            get;
            private set;
        }
        private int _currentPage = -1;

        public PagesLoader(IMercuryClient mercuryClient)
        {
            _mercuryClient = mercuryClient;
            _pages = new List<ContextPage>();
        }

        public static PagesLoader From(IMercuryClient mercury, string contextUri)
            => new PagesLoader(mercury)
            {
                ResolveUrl = contextUri
            };

        public static PagesLoader From(IMercuryClient mercury, Context context)
        {
            var pages = context.Pages.ToList();
            if (!pages.Any()) return From(mercury, context.Uri);


            var loader = new PagesLoader(mercury);
            loader.FirstPages(pages, context.Uri);
            return loader;
        }

        public List<ContextTrack> FetchTracks(string contextUrl)
        {
            var mercuryResponse =
                _mercuryClient.SendSync(new JsonMercuryRequest<string>(RawMercuryRequest.Get(contextUrl)));
            return ProtoUtils.JsonToContextTracks(JObject.Parse(mercuryResponse)["tracks"] as JArray ?? throw new InvalidOperationException());

        }
        public List<ContextTrack> ResolvePage([NotNull] ContextPage page)
        {
            if (page.Tracks.Count > 0)
                return page.Tracks.ToList();

            if (page.HasPageUrl)
            {
                return FetchTracks(page.PageUrl);
            }
            else if (page.HasLoading && page.Loading)
            {
                //??????????????????
                //ガチでどういう意味？
                throw new ArgumentOutOfRangeException();
            }
            else
            {
                throw new IllegalStateException("Could not load page");
            }
        }
        public List<ContextTrack> GetPage(int pageIndex)
        {
            if (pageIndex == -1) throw new ArgumentException($"You must initialize the pages first");

            if (pageIndex == 0 && !_pages.Any() && ResolveUrl != null)
            {
                var resolvedPages = _mercuryClient.SendSync(MercuryRequests.ResolveContext(ResolveUrl));
                var serialized = JObject.Parse(JsonConvert.SerializeObject(resolvedPages.Pages));
                 _pages.AddRange(ProtoUtils.JsonToContextPages(serialized["pages"] as JArray ?? throw new InvalidOperationException()));
            }

            ResolveUrl = null;

            if (pageIndex < _pages.Count)
            {
                var page = _pages[pageIndex];
                var tracks = ResolvePage(page);
                page.ClearPageUrl();
                page.Tracks.Clear();
                page.Tracks.AddRange(tracks);
                _pages[pageIndex] = page;
                return tracks;
            }
            else
            {
                if (pageIndex > _pages.Count) throw new ArgumentOutOfRangeException();

                var previous = _pages[pageIndex - 1];
                if (!previous.HasNextPageUrl) throw new IllegalStateException();

                var nextPageUrl = previous.NextPageUrl;
                previous.ClearNextPageUrl();
                _pages[pageIndex - 1] = previous;

                var tracks = FetchTracks(nextPageUrl);
                var newPage = new ContextPage();
                newPage.Tracks.AddRange(tracks);
                _pages.Add(newPage);
                return tracks;
            }
        }
        public bool NextPage()
        {
            try
            {
                GetPage(_currentPage + 1);
                _currentPage++;
                return true;
            }
            catch (IllegalStateException illegal)
            {
                return false;
            }
        }
        private void FirstPages(IEnumerable<ContextPage> pages, string contextUri)
        {
            if (_currentPage != -1 || this._pages.Any()) throw new IllegalStateException("Pages already initialized.");
            foreach (var page in pages)
            {
                var tracks = page.Tracks.ToList();
                SanitizeTracks(tracks, PlayableId.InferUriPrefix(contextUri));
                page.Tracks.Clear();
                page.Tracks.AddRange(tracks);
                _pages.Add(page);
            }
        }


        private static void SanitizeTracks(IList<ContextTrack> tracks, string uriPrefix)
        {
            for (var i = 0; i < tracks.Count; i++)
            {
                var b = tracks[i];
                if (b.HasUri && !b.Uri.IsEmpty() || !b.HasGid) continue;

                b.Uri = $"{uriPrefix}{Base62Test.CreateInstanceWithInvertedCharacterSet().Encode(b.Gid.ToByteArray())}";
                tracks[i] = b;
            }
        }
    }
}
