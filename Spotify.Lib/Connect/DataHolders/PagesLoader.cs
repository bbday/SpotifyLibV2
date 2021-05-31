using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using Spotify.Lib.Exceptions;
using Spotify.Lib.Helpers;
using Spotify.Lib.Models;
using Spotify.Player.Proto;

namespace Spotify.Lib.Connect.DataHolders
{
    internal static class PagesHelper
    {
        internal static bool NextPage(ref Pages pages)
        {
            try
            {
                GetPage(ref pages, pages.CurrentPageIndex + 1);
                pages.CurrentPageIndex++;
                return true;
            }
            catch (IllegalStateException illegal)
            {
                return false;
            }
        }
        internal static List<ContextTrack> GetPage(ref Pages pages,
            int pageIndex)
        {
            if (pageIndex == -1) throw new ArgumentException($"You must initialize the pages first");

            if (pageIndex == 0 && !pages.Any && pages.ResolveUrl != null)
            {
                var resolveUrl = pages.ResolveUrl;
                var resolvedPages = AsyncContext.Run(() =>
                    SpotifyClient.Instance.SendAsyncReturnJson<MercuryContextWrapperResponse>(RawMercuryRequest.Get(
                        $"hm://context-resolve/v1/{resolveUrl}"), CancellationToken.None, true));
                //     var serialized = JObject.Parse(JsonConvert.SerializeObject(resolvedPages.Pages));
                var str = resolvedPages.Pages;
                pages.ResolvedPages
                    .AddRange(ProtoUtils.JsonToContextPages((JArray)str
                                                            ?? throw new InvalidOperationException()));
            }

            pages.ResolveUrl = null;

            if (pageIndex < pages.Count)
            {
                var page = pages[pageIndex];
                var tracks = ResolvePage(page);
                page.ClearPageUrl();
                page.Tracks.Clear();
                page.Tracks.AddRange(tracks);
                pages[pageIndex] = page;
                return tracks;
            }
            else
            {
                if (pageIndex > pages.Count) throw new ArgumentOutOfRangeException();

                var previous = pages[pageIndex - 1];
                if (!previous.HasNextPageUrl) 
                    throw new IllegalStateException();

                var nextPageUrl = previous.NextPageUrl;
                previous.ClearNextPageUrl();
                pages[pageIndex - 1] = previous;

                var tracks = FetchTracks(nextPageUrl);
                var newPage = new ContextPage();
                newPage.Tracks.AddRange(tracks);
                pages.ResolvedPages.Add(newPage);
                return tracks;
            }
        }
        internal static List<ContextTrack> ResolvePage(ContextPage page)
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
        internal static List<ContextTrack> FetchTracks(string contextUrl)
        {
            var mercuryResponse =
                AsyncContext.Run(() =>
                    SpotifyClient.Instance.SendAsyncReturnJson<string>(RawMercuryRequest.Get(contextUrl),
                        CancellationToken.None));
            return ProtoUtils.JsonToContextTracks(JObject.Parse(mercuryResponse)["tracks"] as JArray ??
                                                  throw new InvalidOperationException());
        }
    }

    public struct Pages 
    {
        public readonly List<ContextPage> ResolvedPages;

        public int CurrentPageIndex
        {
            get;
            set;
        }

        public static Pages 
            From(Spotify.Player.Proto.Context context)
        {
            var pages = context.Pages.ToList();
            if (!pages.Any()) return new Pages(context.Uri);

            var loader = new Pages(pages, context.Uri);
            return loader;
        }

        public List<ContextTrack> CurrentPage => PagesHelper.GetPage(ref this, CurrentPageIndex);

        public Pages(string contextUrl)
        {
            ResolvedPages = new List<ContextPage>();
            ResolveUrl = contextUrl;
            CurrentPageIndex = -1;
        }
        public Pages(List<ContextPage> pages, string contextUri)
        {
            ResolvedPages = pages;
            ResolveUrl = contextUri;
            CurrentPageIndex = -1;
        }

        public string ResolveUrl;
        public ContextPage this[int index]
        {
            get { return ResolvedPages[index]; }
            set { ResolvedPages.Insert(index, value); }
        }

        public bool Any => ResolvedPages.Any();
        public int Count => ResolvedPages.Count;
    }
}
