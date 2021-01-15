// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: play_origin.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Spotify.Player.Proto {

  /// <summary>Holder for reflection information generated from play_origin.proto</summary>
  public static partial class PlayOriginReflection {

    #region Descriptor
    /// <summary>File descriptor for play_origin.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PlayOriginReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFwbGF5X29yaWdpbi5wcm90bxIUc3BvdGlmeS5wbGF5ZXIucHJvdG8ivwEK",
            "ClBsYXlPcmlnaW4SGgoSZmVhdHVyZV9pZGVudGlmaWVyGAEgASgJEhcKD2Zl",
            "YXR1cmVfdmVyc2lvbhgCIAEoCRIQCgh2aWV3X3VyaRgDIAEoCRIZChFleHRl",
            "cm5hbF9yZWZlcnJlchgEIAEoCRIbChNyZWZlcnJlcl9pZGVudGlmaWVyGAUg",
            "ASgJEhkKEWRldmljZV9pZGVudGlmaWVyGAYgASgJEhcKD2ZlYXR1cmVfY2xh",
            "c3NlcxgHIAMoCUIXChNjb20uc3BvdGlmeS5jb250ZXh0SAI="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Spotify.Player.Proto.PlayOrigin), global::Spotify.Player.Proto.PlayOrigin.Parser, new[]{ "FeatureIdentifier", "FeatureVersion", "ViewUri", "ExternalReferrer", "ReferrerIdentifier", "DeviceIdentifier", "FeatureClasses" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class PlayOrigin : pb::IMessage<PlayOrigin>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PlayOrigin> _parser = new pb::MessageParser<PlayOrigin>(() => new PlayOrigin());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PlayOrigin> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Spotify.Player.Proto.PlayOriginReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PlayOrigin() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PlayOrigin(PlayOrigin other) : this() {
      featureIdentifier_ = other.featureIdentifier_;
      featureVersion_ = other.featureVersion_;
      viewUri_ = other.viewUri_;
      externalReferrer_ = other.externalReferrer_;
      referrerIdentifier_ = other.referrerIdentifier_;
      deviceIdentifier_ = other.deviceIdentifier_;
      featureClasses_ = other.featureClasses_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PlayOrigin Clone() {
      return new PlayOrigin(this);
    }

    /// <summary>Field number for the "feature_identifier" field.</summary>
    public const int FeatureIdentifierFieldNumber = 1;
    private readonly static string FeatureIdentifierDefaultValue = "";

    private string featureIdentifier_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string FeatureIdentifier {
      get { return featureIdentifier_ ?? FeatureIdentifierDefaultValue; }
      set {
        featureIdentifier_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "feature_identifier" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasFeatureIdentifier {
      get { return featureIdentifier_ != null; }
    }
    /// <summary>Clears the value of the "feature_identifier" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearFeatureIdentifier() {
      featureIdentifier_ = null;
    }

    /// <summary>Field number for the "feature_version" field.</summary>
    public const int FeatureVersionFieldNumber = 2;
    private readonly static string FeatureVersionDefaultValue = "";

    private string featureVersion_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string FeatureVersion {
      get { return featureVersion_ ?? FeatureVersionDefaultValue; }
      set {
        featureVersion_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "feature_version" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasFeatureVersion {
      get { return featureVersion_ != null; }
    }
    /// <summary>Clears the value of the "feature_version" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearFeatureVersion() {
      featureVersion_ = null;
    }

    /// <summary>Field number for the "view_uri" field.</summary>
    public const int ViewUriFieldNumber = 3;
    private readonly static string ViewUriDefaultValue = "";

    private string viewUri_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ViewUri {
      get { return viewUri_ ?? ViewUriDefaultValue; }
      set {
        viewUri_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "view_uri" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasViewUri {
      get { return viewUri_ != null; }
    }
    /// <summary>Clears the value of the "view_uri" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearViewUri() {
      viewUri_ = null;
    }

    /// <summary>Field number for the "external_referrer" field.</summary>
    public const int ExternalReferrerFieldNumber = 4;
    private readonly static string ExternalReferrerDefaultValue = "";

    private string externalReferrer_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ExternalReferrer {
      get { return externalReferrer_ ?? ExternalReferrerDefaultValue; }
      set {
        externalReferrer_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "external_referrer" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasExternalReferrer {
      get { return externalReferrer_ != null; }
    }
    /// <summary>Clears the value of the "external_referrer" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearExternalReferrer() {
      externalReferrer_ = null;
    }

    /// <summary>Field number for the "referrer_identifier" field.</summary>
    public const int ReferrerIdentifierFieldNumber = 5;
    private readonly static string ReferrerIdentifierDefaultValue = "";

    private string referrerIdentifier_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ReferrerIdentifier {
      get { return referrerIdentifier_ ?? ReferrerIdentifierDefaultValue; }
      set {
        referrerIdentifier_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "referrer_identifier" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasReferrerIdentifier {
      get { return referrerIdentifier_ != null; }
    }
    /// <summary>Clears the value of the "referrer_identifier" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearReferrerIdentifier() {
      referrerIdentifier_ = null;
    }

    /// <summary>Field number for the "device_identifier" field.</summary>
    public const int DeviceIdentifierFieldNumber = 6;
    private readonly static string DeviceIdentifierDefaultValue = "";

    private string deviceIdentifier_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string DeviceIdentifier {
      get { return deviceIdentifier_ ?? DeviceIdentifierDefaultValue; }
      set {
        deviceIdentifier_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "device_identifier" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasDeviceIdentifier {
      get { return deviceIdentifier_ != null; }
    }
    /// <summary>Clears the value of the "device_identifier" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearDeviceIdentifier() {
      deviceIdentifier_ = null;
    }

    /// <summary>Field number for the "feature_classes" field.</summary>
    public const int FeatureClassesFieldNumber = 7;
    private static readonly pb::FieldCodec<string> _repeated_featureClasses_codec
        = pb::FieldCodec.ForString(58);
    private readonly pbc::RepeatedField<string> featureClasses_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<string> FeatureClasses {
      get { return featureClasses_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PlayOrigin);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PlayOrigin other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (FeatureIdentifier != other.FeatureIdentifier) return false;
      if (FeatureVersion != other.FeatureVersion) return false;
      if (ViewUri != other.ViewUri) return false;
      if (ExternalReferrer != other.ExternalReferrer) return false;
      if (ReferrerIdentifier != other.ReferrerIdentifier) return false;
      if (DeviceIdentifier != other.DeviceIdentifier) return false;
      if(!featureClasses_.Equals(other.featureClasses_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasFeatureIdentifier) hash ^= FeatureIdentifier.GetHashCode();
      if (HasFeatureVersion) hash ^= FeatureVersion.GetHashCode();
      if (HasViewUri) hash ^= ViewUri.GetHashCode();
      if (HasExternalReferrer) hash ^= ExternalReferrer.GetHashCode();
      if (HasReferrerIdentifier) hash ^= ReferrerIdentifier.GetHashCode();
      if (HasDeviceIdentifier) hash ^= DeviceIdentifier.GetHashCode();
      hash ^= featureClasses_.GetHashCode();
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
      if (HasFeatureIdentifier) {
        output.WriteRawTag(10);
        output.WriteString(FeatureIdentifier);
      }
      if (HasFeatureVersion) {
        output.WriteRawTag(18);
        output.WriteString(FeatureVersion);
      }
      if (HasViewUri) {
        output.WriteRawTag(26);
        output.WriteString(ViewUri);
      }
      if (HasExternalReferrer) {
        output.WriteRawTag(34);
        output.WriteString(ExternalReferrer);
      }
      if (HasReferrerIdentifier) {
        output.WriteRawTag(42);
        output.WriteString(ReferrerIdentifier);
      }
      if (HasDeviceIdentifier) {
        output.WriteRawTag(50);
        output.WriteString(DeviceIdentifier);
      }
      featureClasses_.WriteTo(output, _repeated_featureClasses_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasFeatureIdentifier) {
        output.WriteRawTag(10);
        output.WriteString(FeatureIdentifier);
      }
      if (HasFeatureVersion) {
        output.WriteRawTag(18);
        output.WriteString(FeatureVersion);
      }
      if (HasViewUri) {
        output.WriteRawTag(26);
        output.WriteString(ViewUri);
      }
      if (HasExternalReferrer) {
        output.WriteRawTag(34);
        output.WriteString(ExternalReferrer);
      }
      if (HasReferrerIdentifier) {
        output.WriteRawTag(42);
        output.WriteString(ReferrerIdentifier);
      }
      if (HasDeviceIdentifier) {
        output.WriteRawTag(50);
        output.WriteString(DeviceIdentifier);
      }
      featureClasses_.WriteTo(ref output, _repeated_featureClasses_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasFeatureIdentifier) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(FeatureIdentifier);
      }
      if (HasFeatureVersion) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(FeatureVersion);
      }
      if (HasViewUri) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ViewUri);
      }
      if (HasExternalReferrer) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ExternalReferrer);
      }
      if (HasReferrerIdentifier) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ReferrerIdentifier);
      }
      if (HasDeviceIdentifier) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(DeviceIdentifier);
      }
      size += featureClasses_.CalculateSize(_repeated_featureClasses_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PlayOrigin other) {
      if (other == null) {
        return;
      }
      if (other.HasFeatureIdentifier) {
        FeatureIdentifier = other.FeatureIdentifier;
      }
      if (other.HasFeatureVersion) {
        FeatureVersion = other.FeatureVersion;
      }
      if (other.HasViewUri) {
        ViewUri = other.ViewUri;
      }
      if (other.HasExternalReferrer) {
        ExternalReferrer = other.ExternalReferrer;
      }
      if (other.HasReferrerIdentifier) {
        ReferrerIdentifier = other.ReferrerIdentifier;
      }
      if (other.HasDeviceIdentifier) {
        DeviceIdentifier = other.DeviceIdentifier;
      }
      featureClasses_.Add(other.featureClasses_);
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
            FeatureIdentifier = input.ReadString();
            break;
          }
          case 18: {
            FeatureVersion = input.ReadString();
            break;
          }
          case 26: {
            ViewUri = input.ReadString();
            break;
          }
          case 34: {
            ExternalReferrer = input.ReadString();
            break;
          }
          case 42: {
            ReferrerIdentifier = input.ReadString();
            break;
          }
          case 50: {
            DeviceIdentifier = input.ReadString();
            break;
          }
          case 58: {
            featureClasses_.AddEntriesFrom(input, _repeated_featureClasses_codec);
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
            FeatureIdentifier = input.ReadString();
            break;
          }
          case 18: {
            FeatureVersion = input.ReadString();
            break;
          }
          case 26: {
            ViewUri = input.ReadString();
            break;
          }
          case 34: {
            ExternalReferrer = input.ReadString();
            break;
          }
          case 42: {
            ReferrerIdentifier = input.ReadString();
            break;
          }
          case 50: {
            DeviceIdentifier = input.ReadString();
            break;
          }
          case 58: {
            featureClasses_.AddEntriesFrom(ref input, _repeated_featureClasses_codec);
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