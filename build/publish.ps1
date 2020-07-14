# Builds the software and creates release artifacts

# Build and create artifacts
dotnet publish --configuration Release --framework netcoreapp3.1 --self-contained false --force --output .\publish\netcoreapp3.1 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj
dotnet publish --configuration Release  --framework net48 --force --output .\publish\net48 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj

# Get the version from the published executable
$version = (Get-Item .\publish\netcoreapp3.1\RadeonSoftwareSlimmer.exe).VersionInfo.ProductVersion

# Archive the artifacts
Compress-Archive -Path .\publish\netcoreapp3.1\* -DestinationPath ".\Publish\RadeonSoftwareSlimmer_${version}_netcoreapp31.zip"
Compress-Archive -Path .\publish\net48\* -DestinationPath ".\Publish\RadeonSoftwareSlimmer_${version}_net48.zip"

# Output the version
Write-Host "Published: $version"