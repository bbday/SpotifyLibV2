// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: extension_kind.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Spotify.Extendedmetadata.Proto {

  /// <summary>Holder for reflection information generated from extension_kind.proto</summary>
  public static partial class ExtensionKindReflection {

    #region Descriptor
    /// <summary>File descriptor for extension_kind.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ExtensionKindReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChRleHRlbnNpb25fa2luZC5wcm90bxIec3BvdGlmeS5leHRlbmRlZG1ldGFk",
            "YXRhLnByb3RvKqcDCg1FeHRlbnNpb25LaW5kEhUKEVVOS05PV05fRVhURU5T",
            "SU9OEAASCgoGQ0FOVkFaEAESDgoKU1RPUllMSU5FUxACEhIKDlBPRENBU1Rf",
            "VE9QSUNTEAMSFAoQUE9EQ0FTVF9TRUdNRU5UUxAEEg8KC0FVRElPX0ZJTEVT",
            "EAUSFAoQVFJBQ0tfREVTQ1JJUFRPUhAGEg0KCUFSVElTVF9WNBAIEgwKCEFM",
            "QlVNX1Y0EAkSDAoIVFJBQ0tfVjQQChILCgdTSE9XX1Y0EAsSDgoKRVBJU09E",
            "RV9WNBAMEhwKGFBPRENBU1RfSFRNTF9ERVNDUklQVElPThANEhIKDlBPRENB",
            "U1RfUVVPVEVTEA4SEAoMVVNFUl9QUk9GSUxFEA8SDQoJQ0FOVkFTX1YxEBAS",
            "EAoMU0hPV19WNF9CQVNFEBESGgoWU0hPV19WNF9FUElTT0RFU19BU1NPQxAS",
            "Eh8KG1RSQUNLX0RFU0NSSVBUT1JfU0lHTkFUVVJFUxATEhcKE1BPRENBU1Rf",
            "QURfU0VHTUVOVFMQFBIPCgtUUkFOU0NSSVBUUxAVQiAKHGNvbS5zcG90aWZ5",
            "LmV4dGVuZGVkbWV0YWRhdGFIAg=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Spotify.Extendedmetadata.Proto.ExtensionKind), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum ExtensionKind {
    [pbr::OriginalName("UNKNOWN_EXTENSION")] UnknownExtension = 0,
    [pbr::OriginalName("CANVAZ")] Canvaz = 1,
    [pbr::OriginalName("STORYLINES")] Storylines = 2,
    [pbr::OriginalName("PODCAST_TOPICS")] PodcastTopics = 3,
    [pbr::OriginalName("PODCAST_SEGMENTS")] PodcastSegments = 4,
    [pbr::OriginalName("AUDIO_FILES")] AudioFiles = 5,
    [pbr::OriginalName("TRACK_DESCRIPTOR")] TrackDescriptor = 6,
    [pbr::OriginalName("ARTIST_V4")] ArtistV4 = 8,
    [pbr::OriginalName("ALBUM_V4")] AlbumV4 = 9,
    [pbr::OriginalName("TRACK_V4")] TrackV4 = 10,
    [pbr::OriginalName("SHOW_V4")] ShowV4 = 11,
    [pbr::OriginalName("EPISODE_V4")] EpisodeV4 = 12,
    [pbr::OriginalName("PODCAST_HTML_DESCRIPTION")] PodcastHtmlDescription = 13,
    [pbr::OriginalName("PODCAST_QUOTES")] PodcastQuotes = 14,
    [pbr::OriginalName("USER_PROFILE")] UserProfile = 15,
    [pbr::OriginalName("CANVAS_V1")] CanvasV1 = 16,
    [pbr::OriginalName("SHOW_V4_BASE")] ShowV4Base = 17,
    [pbr::OriginalName("SHOW_V4_EPISODES_ASSOC")] ShowV4EpisodesAssoc = 18,
    [pbr::OriginalName("TRACK_DESCRIPTOR_SIGNATURES")] TrackDescriptorSignatures = 19,
    [pbr::OriginalName("PODCAST_AD_SEGMENTS")] PodcastAdSegments = 20,
    [pbr::OriginalName("TRANSCRIPTS")] Transcripts = 21,
  }

  #endregion

}

#endregion Designer generated code
