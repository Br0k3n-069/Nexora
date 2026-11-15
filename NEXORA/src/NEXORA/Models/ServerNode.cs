using System;
using System.Runtime.Serialization;

namespace NEXORA.Models
{
    [DataContract]
    public sealed class ServerNode
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string Protocol { get; set; }
        [DataMember] public string Host { get; set; }
        [DataMember] public int Port { get; set; }
        [DataMember] public string SourceId { get; set; }
        [DataMember] public string Uri { get; set; }
        [DataMember] public string ConfigFile { get; set; }
        [DataMember] public DateTime ImportedAt { get; set; }

        public ServerNode()
        {
            Id = Guid.NewGuid().ToString("N");
            ImportedAt = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return Name + "  ·  " + Protocol + "  ·  " + Host + ":" + Port;
        }
    }
}