# Builds the software and creates release artifacts

# Get the version from GetVersion environmental variable
$version = $env:GitVersion_SemVer

# Build and create artifacts
dotnet publish --configuration Release --framework net7.0-windows --self-contained false --force --output .\publish\net70 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:Version=$version
dotnet publish --configuration Release --framework net6.0-windows --self-contained false --force --output .\publish\net60 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:Version=$version
dotnet publish --configuration Release  --framework net48 --force --output .\publish\net48 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:Version=$version

# Output ProductVersion
$productVersion = (Get-Item .\publish\net70\RadeonSoftwareSlimmer.exe).VersionInfo.ProductVersion
Write-Host "ProductVersion: $productVersion"

# Archive the artifacts
Compress-Archive -Path .\publish\net70\* -DestinationPath ".\publish\RadeonSoftwareSlimmer_${version}_net70.zip"
Compress-Archive -Path .\publish\net60\* -DestinationPath ".\publish\RadeonSoftwareSlimmer_${version}_net60.zip"
Compress-Archive -Path .\publish\net48\* -DestinationPath ".\publish\RadeonSoftwareSlimmer_${version}_net48.zip"

# Output the version
Write-Host "Published: $version"