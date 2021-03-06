// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: recently_played.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Spotify.RecentlyPlayed.Proto {

  /// <summary>Holder for reflection information generated from recently_played.proto</summary>
  public static partial class RecentlyPlayedReflection {

    #region Descriptor
    /// <summary>File descriptor for recently_played.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static RecentlyPlayedReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVyZWNlbnRseV9wbGF5ZWQucHJvdG8SHXNwb3RpZnkucmVjZW50bHlfcGxh",
            "eWVkLnByb3RvIlUKB0NvbnRleHQSFAoMY29udGV4dF9saW5rGAEgASgJEhEK",
            "CWl0ZW1fbGluaxgCIAEoCRIRCgl0aW1lc3RhbXAYBCABKAUSDgoGaGlkZGVu",
            "GAUgASgIIj4KBVRyYWNrEhIKCnRyYWNrX2xpbmsYASABKAkSEQoJdGltZXN0",
            "YW1wGAIgASgDEg4KBmhpZGRlbhgDIAEoCCJ3CgVDYWNoZRI4Cghjb250ZXh0",
            "cxgBIAMoCzImLnNwb3RpZnkucmVjZW50bHlfcGxheWVkLnByb3RvLkNvbnRl",
            "eHQSNAoGdHJhY2tzGAIgAygLMiQuc3BvdGlmeS5yZWNlbnRseV9wbGF5ZWQu",
            "cHJvdG8uVHJhY2s="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Spotify.RecentlyPlayed.Proto.Context), global::Spotify.RecentlyPlayed.Proto.Context.Parser, new[]{ "ContextLink", "ItemLink", "Timestamp", "Hidden" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Spotify.RecentlyPlayed.Proto.Track), global::Spotify.RecentlyPlayed.Proto.Track.Parser, new[]{ "TrackLink", "Timestamp", "Hidden" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Spotify.RecentlyPlayed.Proto.Cache), global::Spotify.RecentlyPlayed.Proto.Cache.Parser, new[]{ "Contexts", "Tracks" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Context : pb::IMessage<Context>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Context> _parser = new pb::MessageParser<Context>(() => new Context());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Context> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Spotify.RecentlyPlayed.Proto.RecentlyPlayedReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Context() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Context(Context other) : this() {
      _hasBits0 = other._hasBits0;
      contextLink_ = other.contextLink_;
      itemLink_ = other.itemLink_;
      timestamp_ = other.timestamp_;
      hidden_ = other.hidden_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Context Clone() {
      return new Context(this);
    }

    /// <summary>Field number for the "context_link" field.</summary>
    public const int ContextLinkFieldNumber = 1;
    private readonly static string ContextLinkDefaultValue = "";

    private string contextLink_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ContextLink {
      get { return contextLink_ ?? ContextLinkDefaultValue; }
      set {
        contextLink_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "context_link" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasContextLink {
      get { return contextLink_ != null; }
    }
    /// <summary>Clears the value of the "context_link" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearContextLink() {
      contextLink_ = null;
    }

    /// <summary>Field number for the "item_link" field.</summary>
    public const int ItemLinkFieldNumber = 2;
    private readonly static string ItemLinkDefaultValue = "";

    private string itemLink_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ItemLink {
      get { return itemLink_ ?? ItemLinkDefaultValue; }
      set {
        itemLink_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "item_link" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasItemLink {
      get { return itemLink_ != null; }
    }
    /// <summary>Clears the value of the "item_link" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearItemLink() {
      itemLink_ = null;
    }

    /// <summary>Field number for the "timestamp" field.</summary>
    public const int TimestampFieldNumber = 4;
    private readonly static int TimestampDefaultValue = 0;

    private int timestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Timestamp {
      get { if ((_hasBits0 & 1) != 0) { return timestamp_; } else { return TimestampDefaultValue; } }
      set {
        _hasBits0 |= 1;
        timestamp_ = value;
      }
    }
    /// <summary>Gets whether the "timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasTimestamp {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearTimestamp() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "hidden" field.</summary>
    public const int HiddenFieldNumber = 5;
    private readonly static bool HiddenDefaultValue = false;

    private bool hidden_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Hidden {
      get { if ((_hasBits0 & 2) != 0) { return hidden_; } else { return HiddenDefaultValue; } }
      set {
        _hasBits0 |= 2;
        hidden_ = value;
      }
    }
    /// <summary>Gets whether the "hidden" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasHidden {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "hidden" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearHidden() {
      _hasBits0 &= ~2;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Context);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Context other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ContextLink != other.ContextLink) return false;
      if (ItemLink != other.ItemLink) return false;
      if (Timestamp != other.Timestamp) return false;
      if (Hidden != other.Hidden) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasContextLink) hash ^= ContextLink.GetHashCode();
      if (HasItemLink) hash ^= ItemLink.GetHashCode();
      if (HasTimestamp) hash ^= Timestamp.GetHashCode();
      if (HasHidden) hash ^= Hidden.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (HasContextLink) {
        output.WriteRawTag(10);
        output.WriteString(ContextLink);
      }
      if (HasItemLink) {
        output.WriteRawTag(18);
        output.WriteString(ItemLink);
      }
      if (HasTimestamp) {
        output.WriteRawTag(32);
        output.WriteInt32(Timestamp);
      }
      if (HasHidden) {
        output.WriteRawTag(40);
        output.WriteBool(Hidden);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasContextLink) {
        output.WriteRawTag(10);
        output.WriteString(ContextLink);
      }
      if (HasItemLink) {
        output.WriteRawTag(18);
        output.WriteString(ItemLink);
      }
      if (HasTimestamp) {
        output.WriteRawTag(32);
        output.WriteInt32(Timestamp);
      }
      if (HasHidden) {
        output.WriteRawTag(40);
        output.WriteBool(Hidden);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasContextLink) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ContextLink);
      }
      if (HasItemLink) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ItemLink);
      }
      if (HasTimestamp) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Timestamp);
      }
      if (HasHidden) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Context other) {
      if (other == null) {
        return;
      }
      if (other.HasContextLink) {
        ContextLink = other.ContextLink;
      }
      if (other.HasItemLink) {
        ItemLink = other.ItemLink;
      }
      if (other.HasTimestamp) {
        Timestamp = other.Timestamp;
      }
      if (other.HasHidden) {
        Hidden = other.Hidden;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            ContextLink = input.ReadString();
            break;
          }
          case 18: {
            ItemLink = input.ReadString();
            break;
          }
          case 32: {
            Timestamp = input.ReadInt32();
            break;
          }
          case 40: {
            Hidden = input.ReadBool();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            ContextLink = input.ReadString();
            break;
          }
          case 18: {
            ItemLink = input.ReadString();
            break;
          }
          case 32: {
            Timestamp = input.ReadInt32();
            break;
          }
          case 40: {
            Hidden = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class Track : pb::IMessage<Track>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Track> _parser = new pb::MessageParser<Track>(() => new Track());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Track> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Spotify.RecentlyPlayed.Proto.RecentlyPlayedReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Track() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Track(Track other) : this() {
      _hasBits0 = other._hasBits0;
      trackLink_ = other.trackLink_;
      timestamp_ = other.timestamp_;
      hidden_ = other.hidden_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Track Clone() {
      return new Track(this);
    }

    /// <summary>Field number for the "track_link" field.</summary>
    public const int TrackLinkFieldNumber = 1;
    private readonly static string TrackLinkDefaultValue = "";

    private string trackLink_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string TrackLink {
      get { return trackLink_ ?? TrackLinkDefaultValue; }
      set {
        trackLink_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "track_link" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasTrackLink {
      get { return trackLink_ != null; }
    }
    /// <summary>Clears the value of the "track_link" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearTrackLink() {
      trackLink_ = null;
    }

    /// <summary>Field number for the "timestamp" field.</summary>
    public const int TimestampFieldNumber = 2;
    private readonly static long TimestampDefaultValue = 0L;

    private long timestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long Timestamp {
      get { if ((_hasBits0 & 1) != 0) { return timestamp_; } else { return TimestampDefaultValue; } }
      set {
        _hasBits0 |= 1;
        timestamp_ = value;
      }
    }
    /// <summary>Gets whether the "timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasTimestamp {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearTimestamp() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "hidden" field.</summary>
    public const int HiddenFieldNumber = 3;
    private readonly static bool HiddenDefaultValue = false;

    private bool hidden_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Hidden {
      get { if ((_hasBits0 & 2) != 0) { return hidden_; } else { return HiddenDefaultValue; } }
      set {
        _hasBits0 |= 2;
        hidden_ = value;
      }
    }
    /// <summary>Gets whether the "hidden" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasHidden {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "hidden" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearHidden() {
      _hasBits0 &= ~2;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Track);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Track other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (TrackLink != other.TrackLink) return false;
      if (Timestamp != other.Timestamp) return false;
      if (Hidden != other.Hidden) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasTrackLink) hash ^= TrackLink.GetHashCode();
      if (HasTimestamp) hash ^= Timestamp.GetHashCode();
      if (HasHidden) hash ^= Hidden.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (HasTrackLink) {
        output.WriteRawTag(10);
        output.WriteString(TrackLink);
      }
      if (HasTimestamp) {
        output.WriteRawTag(16);
        output.WriteInt64(Timestamp);
      }
      if (HasHidden) {
        output.WriteRawTag(24);
        output.WriteBool(Hidden);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasTrackLink) {
        output.WriteRawTag(10);
        output.WriteString(TrackLink);
      }
      if (HasTimestamp) {
        output.WriteRawTag(16);
        output.WriteInt64(Timestamp);
      }
      if (HasHidden) {
        output.WriteRawTag(24);
        output.WriteBool(Hidden);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasTrackLink) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(TrackLink);
      }
      if (HasTimestamp) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Timestamp);
      }
      if (HasHidden) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Track other) {
      if (other == null) {
        return;
      }
      if (other.HasTrackLink) {
        TrackLink = other.TrackLink;
      }
      if (other.HasTimestamp) {
        Timestamp = other.Timestamp;
      }
      if (other.HasHidden) {
        Hidden = other.Hidden;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            TrackLink = input.ReadString();
            break;
          }
          case 16: {
            Timestamp = input.ReadInt64();
            break;
          }
          case 24: {
            Hidden = input.ReadBool();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            TrackLink = input.ReadString();
            break;
          }
          case 16: {
            Timestamp = input.ReadInt64();
            break;
          }
          case 24: {
            Hidden = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class Cache : pb::IMessage<Cache>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Cache> _parser = new pb::MessageParser<Cache>(() => new Cache());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Cache> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Spotify.RecentlyPlayed.Proto.RecentlyPlayedReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Cache() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Cache(Cache other) : this() {
      contexts_ = other.contexts_.Clone();
      tracks_ = other.tracks_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Cache Clone() {
      return new Cache(this);
    }

    /// <summary>Field number for the "contexts" field.</summary>
    public const int ContextsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Spotify.RecentlyPlayed.Proto.Context> _repeated_contexts_codec
        = pb::FieldCodec.ForMessage(10, global::Spotify.RecentlyPlayed.Proto.Context.Parser);
    private readonly pbc::RepeatedField<global::Spotify.RecentlyPlayed.Proto.Context> contexts_ = new pbc::RepeatedField<global::Spotify.RecentlyPlayed.Proto.Context>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Spotify.RecentlyPlayed.Proto.Context> Contexts {
      get { return contexts_; }
    }

    /// <summary>Field number for the "tracks" field.</summary>
    public const int TracksFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Spotify.RecentlyPlayed.Proto.Track> _repeated_tracks_codec
        = pb::FieldCodec.ForMessage(18, global::Spotify.RecentlyPlayed.Proto.Track.Parser);
    private readonly pbc::RepeatedField<global::Spotify.RecentlyPlayed.Proto.Track> tracks_ = new pbc::RepeatedField<global::Spotify.RecentlyPlayed.Proto.Track>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Spotify.RecentlyPlayed.Proto.Track> Tracks {
      get { return tracks_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Cache);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Cache other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!contexts_.Equals(other.contexts_)) return false;
      if(!tracks_.Equals(other.tracks_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= contexts_.GetHashCode();
      hash ^= tracks_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      contexts_.WriteTo(output, _repeated_contexts_codec);
      tracks_.WriteTo(output, _repeated_tracks_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      contexts_.WriteTo(ref output, _repeated_contexts_codec);
      tracks_.WriteTo(ref output, _repeated_tracks_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += contexts_.CalculateSize(_repeated_contexts_codec);
      size += tracks_.CalculateSize(_repeated_tracks_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Cache other) {
      if (other == null) {
        return;
      }
      contexts_.Add(other.contexts_);
      tracks_.Add(other.tracks_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            contexts_.AddEntriesFrom(input, _repeated_contexts_codec);
            break;
          }
          case 18: {
            tracks_.AddEntriesFrom(input, _repeated_tracks_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            contexts_.AddEntriesFrom(ref input, _repeated_contexts_codec);
            break;
          }
          case 18: {
            tracks_.AddEntriesFrom(ref input, _repeated_tracks_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
