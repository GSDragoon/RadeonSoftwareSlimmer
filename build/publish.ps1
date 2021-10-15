# Builds the software and creates release artifacts

# Get the version from GetVersion environmental variable
$version = $env:GitVersion_SemVer

# Build and create artifacts
dotnet publish --configuration Release --framework net5.0-windows --self-contained false --force --output .\publish\net5.0 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:Version=$version
dotnet publish --configuration Release --framework netcoreapp3.1 --self-contained false --force --output .\publish\netcoreapp3.1 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:Version=$version
dotnet publish --configuration Release  --framework net48 --force --output .\publish\net48 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:Version=$version

# Output ProductVersion
$productVersion = (Get-Item .\publish\net5.0\RadeonSoftwareSlimmer.exe).VersionInfo.ProductVersion
Write-Host "ProductVersion: $productVersion"

# Archive the artifacts
Compress-Archive -Path .\publish\net5.0\* -DestinationPath ".\publish\RadeonSoftwareSlimmer_${version}_net50.zip"
Compress-Archive -Path .\publish\netcoreapp3.1\* -DestinationPath ".\publish\RadeonSoftwareSlimmer_${version}_netcoreapp31.zip"
Compress-Archive -Path .\publish\net48\* -DestinationPath ".\publish\RadeonSoftwareSlimmer_${version}_net48.zip"

# Output the version
Write-Host "Published: $version"