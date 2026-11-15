using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using NEXORA.Models;

namespace NEXORA.Services
{
    public sealed class SubscriptionStore
    {
        private readonly string _path;
        public ObservableCollection<SubscriptionSource> Sources { get; private set; }
        public ObservableCollection<ServerNode> Nodes { get; private set; }

        public SubscriptionStore()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NEXORA");
            Directory.CreateDirectory(folder);
            _path = Path.Combine(folder, "subscriptions.json");
            Sources = new ObservableCollection<SubscriptionSource>();
            Nodes = new ObservableCollection<ServerNode>();
            Load();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_path)) return;
                using (var stream = File.OpenRead(_path))
                {
                    var serializer = new DataContractJsonSerializer(typeof(SubscriptionSource[]));
                    var rows = (SubscriptionSource[])serializer.ReadObject(stream);
                    Sources.Clear(); Nodes.Clear();
                    foreach (var source in rows ?? new SubscriptionSource[0])
                    {
                        if (source.Nodes == null) source.Nodes = new System.Collections.Generic.List<ServerNode>();
                        Sources.Add(source);
                        foreach (var node in source.Nodes) Nodes.Add(node);
                    }
                }
            }
            catch
            {
                Sources.Clear(); Nodes.Clear();
            }
        }

        public void Save()
        {
            foreach (var source in Sources)
                source.Nodes = Nodes.Where(n => n.SourceId == source.Id).ToList();
            var serializer = new DataContractJsonSerializer(typeof(SubscriptionSource[]));
            var temp = _path + ".tmp";
            using (var stream = File.Create(temp)) serializer.WriteObject(stream, Sources.ToArray());
            if (File.Exists(_path)) File.Delete(_path);
            File.Move(temp, _path);
        }

        public void AddSource(SubscriptionSource source)
        {
            Sources.Add(source);
            foreach (var node in source.Nodes) Nodes.Add(node);
            Save();
        }

        public void ReplaceNodes(SubscriptionSource source, System.Collections.Generic.IEnumerable<ServerNode> nodes)
        {
            foreach (var old in Nodes.Where(n => n.SourceId == source.Id).ToList()) Nodes.Remove(old);
            source.Nodes = nodes.ToList();
            foreach (var node in source.Nodes) Nodes.Add(node);
            source.LastUpdatedAt = DateTime.UtcNow;
            source.LastError = null;
            Save();
        }

        public void RemoveSource(SubscriptionSource source)
        {
            if (source == null) return;
            Sources.Remove(source);
            foreach (var node in Nodes.Where(n => n.SourceId == source.Id).ToList()) Nodes.Remove(node);
            Save();
        }
    }
}