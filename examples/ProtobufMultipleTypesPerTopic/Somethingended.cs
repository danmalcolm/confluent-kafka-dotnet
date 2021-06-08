// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: somethingended.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Confluent.Kafka.Examples.ProtobufMultipleTypesPerTopic {

  /// <summary>Holder for reflection information generated from somethingended.proto</summary>
  public static partial class SomethingendedReflection {

    #region Descriptor
    /// <summary>File descriptor for somethingended.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SomethingendedReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChRzb21ldGhpbmdlbmRlZC5wcm90bxI2Y29uZmx1ZW50LmthZmthLmV4YW1w",
            "bGVzLnByb3RvYnVmTXVsdGlwbGVUeXBlc1BlclRvcGljIkEKDlNvbWV0aGlu",
            "Z0VuZGVkEgoKAklkGAEgASgJEhIKCkVuZE1lc3NhZ2UYAiABKAkSDwoHRW5k",
            "RGF0ZRgDIAEoCWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Confluent.Kafka.Examples.ProtobufMultipleTypesPerTopic.SomethingEnded), global::Confluent.Kafka.Examples.ProtobufMultipleTypesPerTopic.SomethingEnded.Parser, new[]{ "Id", "EndMessage", "EndDate" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class SomethingEnded : pb::IMessage<SomethingEnded> {
    private static readonly pb::MessageParser<SomethingEnded> _parser = new pb::MessageParser<SomethingEnded>(() => new SomethingEnded());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<SomethingEnded> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Confluent.Kafka.Examples.ProtobufMultipleTypesPerTopic.SomethingendedReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SomethingEnded() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SomethingEnded(SomethingEnded other) : this() {
      id_ = other.id_;
      endMessage_ = other.endMessage_;
      endDate_ = other.endDate_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public SomethingEnded Clone() {
      return new SomethingEnded(this);
    }

    /// <summary>Field number for the "Id" field.</summary>
    public const int IdFieldNumber = 1;
    private string id_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Id {
      get { return id_; }
      set {
        id_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "EndMessage" field.</summary>
    public const int EndMessageFieldNumber = 2;
    private string endMessage_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string EndMessage {
      get { return endMessage_; }
      set {
        endMessage_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "EndDate" field.</summary>
    public const int EndDateFieldNumber = 3;
    private string endDate_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string EndDate {
      get { return endDate_; }
      set {
        endDate_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as SomethingEnded);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(SomethingEnded other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (EndMessage != other.EndMessage) return false;
      if (EndDate != other.EndDate) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id.Length != 0) hash ^= Id.GetHashCode();
      if (EndMessage.Length != 0) hash ^= EndMessage.GetHashCode();
      if (EndDate.Length != 0) hash ^= EndDate.GetHashCode();
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
      if (Id.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Id);
      }
      if (EndMessage.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(EndMessage);
      }
      if (EndDate.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(EndDate);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Id);
      }
      if (EndMessage.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(EndMessage);
      }
      if (EndDate.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(EndDate);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(SomethingEnded other) {
      if (other == null) {
        return;
      }
      if (other.Id.Length != 0) {
        Id = other.Id;
      }
      if (other.EndMessage.Length != 0) {
        EndMessage = other.EndMessage;
      }
      if (other.EndDate.Length != 0) {
        EndDate = other.EndDate;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Id = input.ReadString();
            break;
          }
          case 18: {
            EndMessage = input.ReadString();
            break;
          }
          case 26: {
            EndDate = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
