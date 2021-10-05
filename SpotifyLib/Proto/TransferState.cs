// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: transfer_state.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Spotify.Player.Proto.Transfer {

  /// <summary>Holder for reflection information generated from transfer_state.proto</summary>
  public static partial class TransferStateReflection {

    #region Descriptor
    /// <summary>File descriptor for transfer_state.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TransferStateReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChR0cmFuc2Zlcl9zdGF0ZS5wcm90bxIdc3BvdGlmeS5wbGF5ZXIucHJvdG8u",
            "dHJhbnNmZXIaHGNvbnRleHRfcGxheWVyX29wdGlvbnMucHJvdG8aDnBsYXli",
            "YWNrLnByb3RvGg1zZXNzaW9uLnByb3RvGgtxdWV1ZS5wcm90byKZAgoNVHJh",
            "bnNmZXJTdGF0ZRI7CgdvcHRpb25zGAEgASgLMiouc3BvdGlmeS5wbGF5ZXIu",
            "cHJvdG8uQ29udGV4dFBsYXllck9wdGlvbnMSOQoIcGxheWJhY2sYAiABKAsy",
            "Jy5zcG90aWZ5LnBsYXllci5wcm90by50cmFuc2Zlci5QbGF5YmFjaxI/Cg9j",
            "dXJyZW50X3Nlc3Npb24YAyABKAsyJi5zcG90aWZ5LnBsYXllci5wcm90by50",
            "cmFuc2Zlci5TZXNzaW9uEjMKBXF1ZXVlGAQgASgLMiQuc3BvdGlmeS5wbGF5",
            "ZXIucHJvdG8udHJhbnNmZXIuUXVldWUSGgoSY3JlYXRpb25fdGltZXN0YW1w",
            "GAUgASgDQhgKFGNvbS5zcG90aWZ5LnRyYW5zZmVySAI="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Spotify.Player.Proto.ContextPlayerOptionsReflection.Descriptor, global::Spotify.Player.Proto.Transfer.PlaybackReflection.Descriptor, global::Spotify.Player.Proto.Transfer.SessionReflection.Descriptor, global::Spotify.Player.Proto.Transfer.QueueReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Spotify.Player.Proto.Transfer.TransferState), global::Spotify.Player.Proto.Transfer.TransferState.Parser, new[]{ "Options", "Playback", "CurrentSession", "Queue", "CreationTimestamp" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TransferState : pb::IMessage<TransferState>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TransferState> _parser = new pb::MessageParser<TransferState>(() => new TransferState());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TransferState> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Spotify.Player.Proto.Transfer.TransferStateReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransferState() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransferState(TransferState other) : this() {
      _hasBits0 = other._hasBits0;
      options_ = other.options_ != null ? other.options_.Clone() : null;
      playback_ = other.playback_ != null ? other.playback_.Clone() : null;
      currentSession_ = other.currentSession_ != null ? other.currentSession_.Clone() : null;
      queue_ = other.queue_ != null ? other.queue_.Clone() : null;
      creationTimestamp_ = other.creationTimestamp_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TransferState Clone() {
      return new TransferState(this);
    }

    /// <summary>Field number for the "options" field.</summary>
    public const int OptionsFieldNumber = 1;
    private global::Spotify.Player.Proto.ContextPlayerOptions options_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Spotify.Player.Proto.ContextPlayerOptions Options {
      get { return options_; }
      set {
        options_ = value;
      }
    }

    /// <summary>Field number for the "playback" field.</summary>
    public const int PlaybackFieldNumber = 2;
    private global::Spotify.Player.Proto.Transfer.Playback playback_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Spotify.Player.Proto.Transfer.Playback Playback {
      get { return playback_; }
      set {
        playback_ = value;
      }
    }

    /// <summary>Field number for the "current_session" field.</summary>
    public const int CurrentSessionFieldNumber = 3;
    private global::Spotify.Player.Proto.Transfer.Session currentSession_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Spotify.Player.Proto.Transfer.Session CurrentSession {
      get { return currentSession_; }
      set {
        currentSession_ = value;
      }
    }

    /// <summary>Field number for the "queue" field.</summary>
    public const int QueueFieldNumber = 4;
    private global::Spotify.Player.Proto.Transfer.Queue queue_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Spotify.Player.Proto.Transfer.Queue Queue {
      get { return queue_; }
      set {
        queue_ = value;
      }
    }

    /// <summary>Field number for the "creation_timestamp" field.</summary>
    public const int CreationTimestampFieldNumber = 5;
    private readonly static long CreationTimestampDefaultValue = 0L;

    private long creationTimestamp_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long CreationTimestamp {
      get { if ((_hasBits0 & 1) != 0) { return creationTimestamp_; } else { return CreationTimestampDefaultValue; } }
      set {
        _hasBits0 |= 1;
        creationTimestamp_ = value;
      }
    }
    /// <summary>Gets whether the "creation_timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasCreationTimestamp {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "creation_timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearCreationTimestamp() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TransferState);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TransferState other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Options, other.Options)) return false;
      if (!object.Equals(Playback, other.Playback)) return false;
      if (!object.Equals(CurrentSession, other.CurrentSession)) return false;
      if (!object.Equals(Queue, other.Queue)) return false;
      if (CreationTimestamp != other.CreationTimestamp) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (options_ != null) hash ^= Options.GetHashCode();
      if (playback_ != null) hash ^= Playback.GetHashCode();
      if (currentSession_ != null) hash ^= CurrentSession.GetHashCode();
      if (queue_ != null) hash ^= Queue.GetHashCode();
      if (HasCreationTimestamp) hash ^= CreationTimestamp.GetHashCode();
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
      if (options_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Options);
      }
      if (playback_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Playback);
      }
      if (currentSession_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(CurrentSession);
      }
      if (queue_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Queue);
      }
      if (HasCreationTimestamp) {
        output.WriteRawTag(40);
        output.WriteInt64(CreationTimestamp);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (options_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Options);
      }
      if (playback_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Playback);
      }
      if (currentSession_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(CurrentSession);
      }
      if (queue_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Queue);
      }
      if (HasCreationTimestamp) {
        output.WriteRawTag(40);
        output.WriteInt64(CreationTimestamp);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (options_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Options);
      }
      if (playback_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Playback);
      }
      if (currentSession_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(CurrentSession);
      }
      if (queue_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Queue);
      }
      if (HasCreationTimestamp) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(CreationTimestamp);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TransferState other) {
      if (other == null) {
        return;
      }
      if (other.options_ != null) {
        if (options_ == null) {
          Options = new global::Spotify.Player.Proto.ContextPlayerOptions();
        }
        Options.MergeFrom(other.Options);
      }
      if (other.playback_ != null) {
        if (playback_ == null) {
          Playback = new global::Spotify.Player.Proto.Transfer.Playback();
        }
        Playback.MergeFrom(other.Playback);
      }
      if (other.currentSession_ != null) {
        if (currentSession_ == null) {
          CurrentSession = new global::Spotify.Player.Proto.Transfer.Session();
        }
        CurrentSession.MergeFrom(other.CurrentSession);
      }
      if (other.queue_ != null) {
        if (queue_ == null) {
          Queue = new global::Spotify.Player.Proto.Transfer.Queue();
        }
        Queue.MergeFrom(other.Queue);
      }
      if (other.HasCreationTimestamp) {
        CreationTimestamp = other.CreationTimestamp;
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
            if (options_ == null) {
              Options = new global::Spotify.Player.Proto.ContextPlayerOptions();
            }
            input.ReadMessage(Options);
            break;
          }
          case 18: {
            if (playback_ == null) {
              Playback = new global::Spotify.Player.Proto.Transfer.Playback();
            }
            input.ReadMessage(Playback);
            break;
          }
          case 26: {
            if (currentSession_ == null) {
              CurrentSession = new global::Spotify.Player.Proto.Transfer.Session();
            }
            input.ReadMessage(CurrentSession);
            break;
          }
          case 34: {
            if (queue_ == null) {
              Queue = new global::Spotify.Player.Proto.Transfer.Queue();
            }
            input.ReadMessage(Queue);
            break;
          }
          case 40: {
            CreationTimestamp = input.ReadInt64();
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
            if (options_ == null) {
              Options = new global::Spotify.Player.Proto.ContextPlayerOptions();
            }
            input.ReadMessage(Options);
            break;
          }
          case 18: {
            if (playback_ == null) {
              Playback = new global::Spotify.Player.Proto.Transfer.Playback();
            }
            input.ReadMessage(Playback);
            break;
          }
          case 26: {
            if (currentSession_ == null) {
              CurrentSession = new global::Spotify.Player.Proto.Transfer.Session();
            }
            input.ReadMessage(CurrentSession);
            break;
          }
          case 34: {
            if (queue_ == null) {
              Queue = new global::Spotify.Player.Proto.Transfer.Queue();
            }
            input.ReadMessage(Queue);
            break;
          }
          case 40: {
            CreationTimestamp = input.ReadInt64();
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