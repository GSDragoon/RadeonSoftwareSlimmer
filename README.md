# Radeon Software Slimmer

![Release](https://github.com/GSDragoon/RadeonSoftwareSlimmer/workflows/Release/badge.svg) 
![Pre-Release](https://github.com/GSDragoon/RadeonSoftwareSlimmer/workflows/Pre-Release/badge.svg) 
![Continuous-Integration](https://github.com/GSDragoon/RadeonSoftwareSlimmer/workflows/Continuous-Integration/badge.svg)

Radeon Software Slimmer is a utility to trim down the "[bloat](https://en.wikipedia.org/wiki/Software_bloat)" with the [Radeon Software](https://www.amd.com/en/technologies/radeon-software) for AMD GPUs on Microsoft Windows.

Radeon Software Adrenalin 2020 Edition introduced a ton of new features. You can read about it [here](https://community.amd.com/community/gaming/blog/2019/12/10/change-the-way-you-game-with-amd-radeon-software-adrenalin-2020-edition). While many enjoy these features, there are some that do not. And to those users, they feel the software contains a lot of unnecessary bloat without any way to not install or disable it. Radeon Software Slimmer is aimed at those users who want to keep their systems as slim as possible. This software is not meant to disrespect AMD or it's hard-working employees. It was inspired by the [NVIDIA driver slimming utility](https://www.guru3d.com/files-details/nvidia-driver-slimming-utility.html).

Radeon Software Slimmer is completely free and open source. It does not contain any advertisements, telemetry or reach out to the internet. Logging is captured within the application, for troubleshooting purposes, but does not write to file or leave the application unless you explicitly do so.

## Disclaimer

This software is **NOT** owned, supported or endorsed by [Advanced Micro Devices, Inc. (AMD)](https://www.amd.com/).

Improper use could cause system instability. **Use at your own risk!**

## Documentation

Documentation can be found on the [Wiki](https://github.com/GSDragoon/RadeonSoftwareSlimmer/wiki).

* [User Guide](https://github.com/GSDragoon/RadeonSoftwareSlimmer/wiki/User-Guide) - Basic guide on how to use Radeon Software Slimmer
* [Dissecting Radeon Software](https://github.com/GSDragoon/RadeonSoftwareSlimmer/wiki/Dissecting-Radeon-Software) - Digging into Radeon Software, how the installer works, what all gets installed and what all the components do.

## Getting Started

Requirements:
* Windows 10 64-bit (Latest version recommended)
* [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48) (Included with Windows 10 1903 and later) **OR** [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1). Separate downloads are provided for each.
* Administrator rights

Installation:
1. Download the version you want on the [Releases page](https://github.com/GSDragoon/RadeonSoftwareSlimmer/releases).
2. Extract the contents of the downloaded zip file to a folder of your choice. If upgrading from a previous version, it is highly recommended to deleted the old files first. There are no persistent configuration files or anything like that.
3. Run `RadeonSoftwareSlimmer.exe`

## Third Party Usage

Thanks to the following third parties used with this software:

* .NET
  * https://dotnet.microsoft.com/
  * Built with .NET Core, .NET Framework and related Microsoft libraries
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