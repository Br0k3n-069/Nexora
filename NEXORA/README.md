# NEXORA 0.2.0

NEXORA is a WPF desktop network-client shell with a restrained graphite interface, local profiles, subscription management and import/export tools.

## Compatibility

- Windows 7 SP1 (requires .NET Framework 4.8 and applicable servicing updates)
- Windows 10 and Windows 11
- WPF / .NET Framework 4.8
- C# 7.3

Modern .NET 10 is not supported on Windows 7. The application targets .NET Framework 4.8 to retain the legacy compatibility target.

## Build

1. Install Visual Studio with the **.NET desktop development** workload.
2. Install the .NET Framework 4.8 Developer Pack.
3. Open `NEXORA.sln`.
4. Select Release / Any CPU and build the solution.

The GitHub Actions workflow builds the solution on a Windows runner.

## Included in 0.2.0

- Graphite WPF shell and compact navigation
- Local profile persistence in `%LOCALAPPDATA%\NEXORA`
- Subscription source manager with local persistence
- HTTP(S) subscription download and manual refresh
- URI parsing for VLESS, VMess, Trojan, Shadowsocks, ShadowsocksR, Hysteria and Hysteria2
- Local file import records for OpenVPN (`.ovpn`) and WireGuard / AmneziaWG (`.conf`)
- Text/JSON preview and local export
- Fade page transitions, animation presets and reduced-motion toggle
- About, changelog and GitHub Releases checker

## Important backend status

**This version does not establish VPN tunnels.** The connection engine remains a deliberate `DemoConnectionEngine` placeholder. Imported configuration records and parsed server URIs are not proof that a protocol can connect. OpenVPN, WireGuard and AmneziaWG files are currently registered as local configuration references; protocol-specific validation, process/service management, routing, DNS handling, kill switch, and real tunnel backends still need implementation and platform testing.

A real integration should use separately versioned and verified backends, validate configuration before launch, report actual process/tunnel state, and never display Connected until the backend confirms it.

## Subscription data and privacy

Subscription URLs and imported nodes may contain credentials or access tokens. They are stored locally under the current user's LocalAppData folder. The export preview is shown before a file is saved; review the content before sharing it. The app does not silently upload exported data.

Subscription downloads are initiated only when the user adds or manually refreshes a URL. Only HTTP and HTTPS URLs are accepted. Responses larger than the configured size limit are rejected.

## Update checker

The update checker queries the GitHub Releases API for this repository. It does not download, replace or install binaries automatically.
