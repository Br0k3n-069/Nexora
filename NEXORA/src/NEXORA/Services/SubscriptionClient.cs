using System;
using System.IO;
using System.Net;
using System.Text;
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
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    if (response.ContentLength > 5 * 1024 * 1024)
                        throw new InvalidOperationException("Subscription response exceeds 5 MB.");
                    return reader.ReadToEnd();
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
            var protocol = ext == ".ovpn" ? "OpenVPN" :
                ext == ".conf" ? "WireGuard / AmneziaWG" : "Configuration";
            return new ServerNode
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Protocol = protocol,
                Host = "Imported configuration",
                Port = 0,
                SourceId = sourceId,
                ConfigFile = path
            };
        }
    }
}