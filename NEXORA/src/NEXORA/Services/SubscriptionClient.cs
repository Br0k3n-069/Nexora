using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NEXORA.Models;

namespace NEXORA.Services
{
    public sealed class SubscriptionClient
    {
        public async Task<string> DownloadAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Subscription URL is empty.");
            var uri = new Uri(url);
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("Only HTTP and HTTPS subscription URLs are supported.");

            return await Task.Run(() =>
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "GET";
                request.UserAgent = "NEXORA/0.2";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 15000;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var memory = new MemoryStream())
                {
                    var buffer = new byte[8192];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (memory.Length + read > 5 * 1024 * 1024)
                            throw new InvalidOperationException("Subscription response exceeds 5 MB.");
                        memory.Write(buffer, 0, read);
                    }
                    return Encoding.UTF8.GetString(memory.ToArray());
                }
            });
        }

        public string ReadConfigFile(string path)
        {
            var info = new FileInfo(path);
            if (!info.Exists) throw new FileNotFoundException("Configuration file not found.", path);
            if (info.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Configuration file exceeds 5 MB.");
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public ServerNode ImportConfigFile(string path, string sourceId)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            var text = ReadConfigFile(path);
            var protocol = "Configuration";
            var host = "Imported configuration";
            var port = 0;

            if (ext == ".ovpn")
            {
                protocol = "OpenVPN";
                var remote = Regex.Match(text, @"(?im)^\s*remote\s+([^\s#]+)(?:\s+(\d+))?");
                if (remote.Success)
                {
                    host = remote.Groups[1].Value;
                    int.TryParse(remote.Groups[2].Value, out port);
                }
            }
            else if (ext == ".conf")
            {
                protocol = Regex.IsMatch(text, @"(?im)^\s*(Jc|Jmin|Jmax|S1|S2|H1|H2)\s*=") ? "AmneziaWG" : "WireGuard";
                var endpoint = Regex.Match(text, @"(?im)^\s*Endpoint\s*=\s*([^:#\s]+)(?::(\d+))?");
                if (endpoint.Success)
                {
                    host = endpoint.Groups[1].Value;
                    int.TryParse(endpoint.Groups[2].Value, out port);
                }
            }

            return new ServerNode
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Protocol = protocol,
                Host = host,
                Port = port,
                SourceId = sourceId,
                ConfigFile = path
            };
        }
    }
}