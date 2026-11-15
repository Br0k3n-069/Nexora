using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NEXORA.Models
{
    [DataContract]
    public sealed class SubscriptionSource
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string Url { get; set; }
        [DataMember] public DateTime AddedAt { get; set; }
        [DataMember] public DateTime? LastUpdatedAt { get; set; }
        [DataMember] public string LastError { get; set; }
        [DataMember] public List<ServerNode> Nodes { get; set; }

        public SubscriptionSource()
        {
            Id = Guid.NewGuid().ToString("N");
            AddedAt = DateTime.UtcNow;
            Nodes = new List<ServerNode>();
        }
    }
}