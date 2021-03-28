using System.Collections.Generic;
using System.Text.Json.Serialization;
using J = Newtonsoft.Json.JsonPropertyAttribute;
using R = Newtonsoft.Json.Required;
using N = Newtonsoft.Json.NullValueHandling;

namespace SpotifyLibrary.Models.Response.Mercury
{
    public class MercuryHub
    {
        [J("root")] public HubRoot Root { get; set; }
    }

    public class HubRoot
    {
        [J("id")] public string Id { get; set; }
        [J("model")] public HubRootModel Model { get; set; }
        [J("views")] public List<RootView> Views { get; set; }
    }

    public class HubRootModel
    {
        [J("tab")] public string Tab { get; set; }
        [J("tabs")] public List<HubTab> Tabs { get; set; }
        [J("title")] public string Title { get; set; }
        [J("modelIdentifier")] public string ModelIdentifier { get; set; }
    }

    public class Impression
    {
        [J("feature_id")] public string FeatureId { get; set; }
        [J("impression_type")] public string ImpressionType { get; set; }
        [J("request_id")] public string RequestId { get; set; }
        [J("section_id")] public string SectionId { get; set; }
    }

    public class HubTab
    {
        [J("id")] public string Id { get; set; }
        [J("title")] public string Title { get; set; }
        [J("uri")] public string Uri { get; set; }
    }

    public class RootView
    {
        [J("id")] public string Id { get; set; }
        [J("model")] public PurpleModel Model { get; set; }
        [J("views")] public List<ViewView> Views { get; set; }
    }

    public class PurpleModel
    {
        [J("id")] public string Id { get; set; }
        [J("modelIdentifier")] public string ModelIdentifier { get; set; }
    }

    public class ViewView
    {
        [J("id")] public string Id { get; set; }
        [J("model")] public FluffyModel Model { get; set; }
        [J("views")] public List<object> Views { get; set; }
    }

    public class FluffyModel
    {
        [J("uri")] public string Uri { get; set; }
        [J("modelIdentifier")] public string ModelIdentifier { get; set; }
    }
}