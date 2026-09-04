$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


$version = $Env:BUILD_VERSION
Write-Output "Version: ${version}"

Write-Output '***** Building solution...'
dotnet build --no-incremental --force --configuration Release -p:Version=$version
Write-Output '***** Done Building solution...'
