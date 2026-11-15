using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using NEXORA.Models;

namespace NEXORA.Services
{
    public static class SubscriptionExporter
    {
        public static string ExportLinks(SubscriptionSource[] sources)
        {
            return string.Join(Environment.NewLine, sources.SelectMany(s =>
                (s.Nodes ?? new System.Collections.Generic.List<ServerNode>())
                .Where(n => !string.IsNullOrWhiteSpace(n.Uri))
                .Select(n => "# " + s.Name + " / " + n.Name + Environment.NewLine + n.Uri)));
        }

        public static string ExportJson(SubscriptionSource[] sources)
        {
            var serializer = new DataContractJsonSerializer(typeof(SubscriptionSource[]));
            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, sources);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}