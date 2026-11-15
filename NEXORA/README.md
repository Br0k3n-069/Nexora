# NEXORA — Windows desktop starter

NEXORA is a WPF desktop application starter focused on a customizable restrained graphite UI with subtle glass-style surfaces,
local profiles, theme controls, and a replaceable connection-engine interface.

## Compatibility target

- Windows 7 SP1 (Basic / Legacy mode; requires .NET Framework 4.8 and applicable Windows updates)
- Windows 10
- Windows 11
- WPF / .NET Framework 4.8
- Visual Studio with the ".NET desktop development" workload and .NET Framework 4.8 Developer Pack

This project targets .NET Framework 4.8, not modern .NET 10. Modern .NET does not support Windows 7.

## Build

1. Install Visual Studio Community.
2. In Visual Studio Installer, select **.NET desktop development**.
3. Ensure the **.NET Framework 4.8 targeting pack / developer pack** is installed.
4. Open `NEXORA.sln`.
5. Select `Release` and `Any CPU`.
6. Choose **Build → Build Solution**.
7. The executable will be in `src/NEXORA/bin/Release/NEXORA.exe`.

For explicit architecture builds, change the project platform to x86 or x64 in Configuration Manager.
Any CPU is suitable for this starter UI, but a real VPN backend may require architecture-specific builds.

## Visual direction

The interface uses a restrained graphite palette, compact navigation, thin separators,
small utility labels, and a muted sage accent. Glass-like surfaces are intentionally subtle
rather than neon or heavily blurred, so the app reads as a practical desktop utility.

## What works in this starter

- WPF shell and navigation
- Dashboard
- Local profile creation and persistence (`%LOCALAPPDATA%\NEXORA\profiles.json`)
- Accent presets
- Panel opacity setting
- Basic layout preset selector
- Animation enable setting
- OS / architecture display
- Connection-engine interface boundary

## Important: VPN engine is not included

The included `DemoConnectionEngine` is intentionally a placeholder. It does not create a VPN tunnel,
change routes, install drivers, or claim that a connection is active. A real backend must be integrated
separately and tested on Windows 7, 10, and 11, including x86/x64 where supported.

## Compatibility notes

Windows 7 support means Windows 7 SP1 with required servicing updates and .NET Framework 4.8.
Some modern Windows 10/11 visual effects (Mica, system backdrop, modern blur APIs) are not available
on Windows 7. NEXORA therefore uses a WPF-rendered approximation in its Legacy mode.

"All editions" means desktop editions that support the required .NET Framework and WPF runtime.
Windows Server, Windows RT, embedded variants, and unsupported/modified Windows images are not
automatically covered.

## Data and privacy

Profile data is stored locally in the current user's LocalAppData folder. This starter does not
send telemetry or contact a remote service.
