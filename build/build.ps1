$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


$version = $Env:BUILD_VERSION
Write-Output "BUILD_VERSION: ${BUILD_VERSION}"

Write-Output '***** Building solution...'
dotnet build --no-incremental --force --configuration Release -p:Version=$version --framework net9.0-windows
dotnet build --no-incremental --force --configuration Release -p:Version=$version --framework net8.0-windows
dotnet build --no-incremental --force --configuration Release -p:Version=$version --framework net48
Write-Output '***** Done Building solution...'
