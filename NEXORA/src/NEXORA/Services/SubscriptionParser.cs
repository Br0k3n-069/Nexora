using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NEXORA.Models;

namespace NEXORA.Services
{
    public static class SubscriptionParser
    {
        private static readonly string[] SupportedSchemes =
        {
            "vless", "vmess", "trojan", "ss", "ssr", "hysteria", "hysteria2"
        };

        public static List<ServerNode> ParseText(string input, string sourceId)
        {
            var result = new List<ServerNode>();
            if (string.IsNullOrWhiteSpace(input)) return result;

            var text = input.Trim();
            var lines = text.Replace("\r", "").Split('\n')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0 && !x.StartsWith("#"))
                .ToList();

            if (lines.Count == 1 && !LooksLikeUri(lines[0]))
            {
                var decoded = TryDecodeBase64(lines[0]);
                if (!string.IsNullOrWhiteSpace(decoded))
                {
                    lines = decoded.Replace("\r", "").Split('\n')
                        .Select(x => x.Trim())
                        .Where(x => x.Length > 0 && !x.StartsWith("#"))
                        .ToList();
                }
            }

            foreach (var line in lines)
            {
                ServerNode node;
                if (TryParseUri(line, sourceId, out node))
                    result.Add(node);
            }

            return result;
        }

        public static bool TryParseUri(string value, string sourceId, out ServerNode node)
        {
            node = null;
            if (string.IsNullOrWhiteSpace(value)) return false;

            Uri uri;
            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri)) return false;

            var scheme = uri.Scheme.ToLowerInvariant();
            if (!SupportedSchemes.Contains(scheme)) return false;

            var host = uri.Host;
            var port = uri.IsDefaultPort ? 0 : uri.Port;
            var userInfo = Uri.UnescapeDataString(uri.UserInfo ?? "");
            var name = Uri.UnescapeDataString(uri.Fragment.TrimStart('#'));
            if (string.IsNullOrWhiteSpace(name)) name = host;

            if (scheme == "vmess")
            {
                var payload = uri.OriginalString.Substring("vmess://".Length).Split('?')[0].Split('#')[0];
                var decoded = TryDecodeBase64(payload);
                if (string.IsNullOrWhiteSpace(decoded)) return false;
                var json = decoded;
                var vmessHost = ReadJsonString(json, "add");
                var vmessPort = ReadJsonString(json, "port");
                var vmessName = ReadJsonString(json, "ps");
                int parsedPort;
                if (string.IsNullOrWhiteSpace(vmessHost) || !int.TryParse(vmessPort, out parsedPort)) return false;
                node = new ServerNode
                {
                    Name = string.IsNullOrWhiteSpace(vmessName) ? vmessHost : vmessName,
                    Protocol = "VMess", Host = vmessHost, Port = parsedPort,
                    SourceId = sourceId, Uri = value.Trim()
                };
                return true;
            }

            if (scheme == "ss")
            {
                if (string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(uri.OriginalString))
                {
                    var raw = uri.OriginalString.Substring(uri.Scheme.Length + 3);
                    var at = raw.LastIndexOf('@');
                    if (at > 0)
                    {
                        var decoded = TryDecodeBase64(raw.Substring(0, at));
                        if (!string.IsNullOrWhiteSpace(decoded))
                        {
                            var split = decoded.LastIndexOf(':');
                            if (split > 0) userInfo = decoded;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(host)) return false;
            node = new ServerNode
            {
                Name = name,
                Protocol = ProtocolName(scheme),
                Host = host,
                Port = port,
                SourceId = sourceId,
                Uri = value.Trim()
            };
            return true;
        }

        public static string ProtocolName(string scheme)
        {
            switch ((scheme ?? "").ToLowerInvariant())
            {
                case "vless": return "VLESS";
                case "vmess": return "VMess";
                case "trojan": return "Trojan";
                case "ss": return "Shadowsocks";
                case "ssr": return "ShadowsocksR";
                case "hysteria": return "Hysteria";
                case "hysteria2": return "Hysteria2";
                default: return (scheme ?? "Unknown").ToUpperInvariant();
            }
        }

        private static bool LooksLikeUri(string value)
        {
            return Regex.IsMatch(value, "^[a-zA-Z][a-zA-Z0-9+.-]*://");
        }

        private static string TryDecodeBase64(string value)
        {
            try
            {
                var clean = value.Trim().Replace('-', '+').Replace('_', '/');
                clean = clean.PadRight(clean.Length + ((4 - clean.Length % 4) % 4), '=');
                return Encoding.UTF8.GetString(Convert.FromBase64String(clean));
            }
            catch { return null; }
        }

        private static string ReadJsonString(string json, string key)
        {
            var match = Regex.Match(json, "[\"']" + Regex.Escape(key) + "[\"']\\s*:\\s*[\"']?([^,\"'}]+)");
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
    }
}