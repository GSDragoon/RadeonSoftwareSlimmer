#!/bin/bash

set -eu


version=BUILD_VERSION
echo"Version: ${version}"


echo '***** Publishing Application...'
dotnet publish ./src/RadeonSoftwareSlimmer/RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net10.0
#dotnet publish ./src/RadeonSoftwareSlimmer/RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net9.0-windows
#dotnet publish ./src/RadeonSoftwareSlimmer/RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net8.0-windows
#dotnet publish ./src/RadeonSoftwareSlimmer/RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net48
echo '***** Done Publishing Application...'


echo '***** Archiving Application...'
publishSrcDir='./artifacts/publish/RadeonSoftwareSlimmer'
publishDestDir='./artifacts/publish'

zip -r "${publishDestDir}/RadeonSoftwareSlimmer_${version}_net90.zip" "${publishSrcDir}/release_net10.0-x64/*"
#zip -r "${publishDestDir}/RadeonSoftwareSlimmer_${version}_net90.zip" "${publishSrcDir}/release_net9.0-windows_win-x64/*"
#zip -r "${publishDestDir}/RadeonSoftwareSlimmer_${version}_net80.zip" "${publishSrcDir}/release_net8.0-windows_win-x64/*"
#zip -r "${publishDestDir}/RadeonSoftwareSlimmer_${version}_net48.zip" "${publishSrcDir}/release_net48_win-x64/*"
echo '***** Done Archiving Application...'


echo '***** Archive Hashes (SHA256):'
sha256sum "${publishDestDir}\RadeonSoftwareSlimmer_*_net*.zip"
