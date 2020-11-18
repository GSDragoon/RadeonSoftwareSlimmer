# Local version of CI to run before committing changes

# Delete old local-ci folder, if it exists
if (Test-Path -Path .\local-ci)
{
	Remove-Item -Path .\local-ci -Recurse -Force
}

# Clean
#dotnet clean --configuration Debug --framework net5.0-windows
dotnet clean --configuration Debug --framework netcoreapp3.1
dotnet clean --configuration Debug --framework net48
#dotnet clean --configuration Release --framework net5.0-windows
dotnet clean --configuration Release --framework netcoreapp3.1
dotnet clean --configuration Release --framework net48

# Test
dotnet test .\test\RadeonSoftwareSlimmer.Test\RadeonSoftwareSlimmer.Test.csproj

# Build and create artifacts
#dotnet publish --configuration Release --framework net5.0-windows --self-contained false --force --output .\local-ci\net5.0 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci
dotnet publish --configuration Release --framework netcoreapp3.1 --self-contained false --force --output .\local-ci\netcoreapp3.1 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci
dotnet publish --configuration Release  --framework net48 --force --output .\local-ci\net48 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci

# Get the version from the published executable
$version = (Get-Item .\local-ci\netcoreapp3.1\RadeonSoftwareSlimmer.exe).VersionInfo.ProductVersion

# Archive the artifacts
#Compress-Archive -Path .\local-ci\net5.0\* -DestinationPath ".\local-ci\RadeonSoftwareSlimmer_${version}_net5.0.zip"
Compress-Archive -Path .\local-ci\netcoreapp3.1\* -DestinationPath ".\local-ci\RadeonSoftwareSlimmer_${version}_netcoreapp31.zip"
Compress-Archive -Path .\local-ci\net48\* -DestinationPath ".\local-ci\RadeonSoftwareSlimmer_${version}_net48.zip"

# Output the version
Write-Host "Published: $version"

# Wait for keypress so the output can be read
Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');