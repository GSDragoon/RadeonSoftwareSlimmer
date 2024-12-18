$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


$version = $Env:BUILD_VERSION
Write-Output "BUILD_VERSION: ${BUILD_VERSION}"


Write-Output '***** Publishing Application...'
dotnet publish .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net9.0-windows
dotnet publish .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net8.0-windows
dotnet publish .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj --force --configuration Release -p:Version=$version --framework net48
Write-Output '***** Done Publishing Application...'


Write-Output '***** Archiving Application...'
$publishSrcDir  = '.\artifacts\publish\RadeonSoftwareSlimmer'
$publishDestDir = '.\artifacts\publish'

Compress-Archive -Path "${publishSrcDir}\release_net9.0-windows_win-x64\*" -DestinationPath "${publishDestDir}\RadeonSoftwareSlimmer_${version}_net90.zip"
Compress-Archive -Path "${publishSrcDir}\release_net8.0-windows_win-x64\*" -DestinationPath "${publishDestDir}\RadeonSoftwareSlimmer_${version}_net80.zip"
Compress-Archive -Path "${publishSrcDir}\release_net48_win-x64\*" -DestinationPath "${publishDestDir}\RadeonSoftwareSlimmer_${version}_net48.zip"
Write-Output '***** Done Archiving Application...'
