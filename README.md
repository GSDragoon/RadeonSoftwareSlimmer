# Radeon Software Slimmer

**https://github.com/GSDragoon/RadeonSoftwareSlimmer**

![Release](https://github.com/GSDragoon/RadeonSoftwareSlimmer/workflows/Release/badge.svg) 
![Continuous-Integration](https://github.com/GSDragoon/RadeonSoftwareSlimmer/workflows/Continuous-Integration/badge.svg)

![Latest-Release-Version](https://img.shields.io/github/v/release/GSDragoon/RadeonSoftwareSlimmer?color=yellow)
![Latest-Release-Date](https://img.shields.io/github/release-date/GSDragoon/RadeonSoftwareSlimmer)
![Total-Downloads](https://img.shields.io/github/downloads/GSDragoon/RadeonSoftwareSlimmer/total?color=blue)
![Latest-Downloads](https://img.shields.io/github/downloads/GSDragoon/RadeonSoftwareSlimmer/latest/total?color=blue)

***

Radeon Software Slimmer is a utility to trim down the "[bloat](https://en.wikipedia.org/wiki/Software_bloat)" with the [Radeon Software](https://www.amd.com/en/technologies/radeon-software) for AMD GPUs on Microsoft Windows.

Radeon Software Adrenalin 2020 Edition introduced a ton of new features. You can read about it [here](https://community.amd.com/community/gaming/blog/2019/12/10/change-the-way-you-game-with-amd-radeon-software-adrenalin-2020-edition). While many enjoy these features, there are some that do not. And to those users, they feel the software contains a lot of unnecessary bloat without any way to not install or disable it. Radeon Software Slimmer is aimed at those users who want to keep their systems as slim as possible. This software is not meant to disrespect AMD or it's hard-working employees. It was inspired by the [NVIDIA driver slimming utility](https://www.guru3d.com/files-details/nvidia-driver-slimming-utility.html).

Radeon Software Slimmer is completely free and open source. It does not contain any advertisements, telemetry or reach out to the internet. Logging is captured within the application, for troubleshooting purposes, but does not write to file or leave the application unless you explicitly do so.

## Disclaimer

This software is **NOT** owned, supported or endorsed by [Advanced Micro Devices, Inc. (AMD)](https://www.amd.com/).

Improper use could cause system instability. **Use at your own risk!**

## Getting Started

Documentation can be found on the [Wiki](https://github.com/GSDragoon/RadeonSoftwareSlimmer/wiki).

Requirements:
* Administrator rights
* Windows 10 64-bit or Windows 11 (Latest version recommended)
* .NET (Either ***one*** of the following)
  * [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48) (Included with Windows 10 1903 [May 2019 update] and later)
  * [.NET Framework 4.8.1 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481) (Included with Windows 11 22H2 and later)
  * [.NET Desktop Runtime 6.0 x64](https://dotnet.microsoft.com/download/dotnet/6.0) (latest release recommended)
  * [.NET Desktop Runtime 7.0 x64](https://dotnet.microsoft.com/download/dotnet/7.0) (latest release recommended)

Installation:
1. Download the version you want on the [Releases page](https://github.com/GSDragoon/RadeonSoftwareSlimmer/releases).
2. Extract the contents of the downloaded zip file to a folder of your choice. If upgrading from a previous version, it is highly recommended to deleted the old files first. There are no persistent configuration files or anything like that.
3. Run `RadeonSoftwareSlimmer.exe`

## Third Party Usage

Thanks to the following third parties used with this software:

* .NET
  * https://dotnet.microsoft.com/
  * Built with .NET and related Microsoft libraries
* 7-Zip
  * https://www.7-zip.org/
  * 7z.exe included in download
  * Used for decompressing the Radeon Software installer files
* Radeon profile icon
  * http://www.iconarchive.com/show/papirus-apps-icons-by-papirus-team/radeon-profile-icon.html
  * Main application icon
* MahApps
  * https://mahapps.com/
  * UI controls, styles, themes and more
* Json.NET
  * https://www.newtonsoft.com/json
  * Used for reading json files with the Radeon Software installer
* Task Scheduler Manged Wrapper
  * https://github.com/dahall/taskscheduler
  * Used to read and modify system scheduled tasks
* System.IO.Abstractions
  * https://github.com/System-IO-Abstractions/System.IO.Abstractions
  * Provides abstractions for System.IO for testability
* Shields.io
  * https://shields.io/
  * Provides badges on readme