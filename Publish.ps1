if (Test-Path -Path .\Publish)
{
	Remove-Item -Path .\Publish -Recurse -Force
}

dotnet clean --configuration Release --framework netcoreapp3.1
dotnet clean --configuration Release --framework net48

dotnet publish --configuration Release --framework netcoreapp3.1 --self-contained false --force --output .\Publish\netcoreapp3.1 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj
dotnet publish --configuration Release  --framework net48 --force --output .\Publish\net48 .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj

$version = (Get-Item .\Publish\netcoreapp3.1\RadeonSoftwareSlimmer.exe).VersionInfo.ProductVersion

Compress-Archive -Path .\Publish\netcoreapp3.1\* -DestinationPath ".\Publish\RadeonSoftwareSlimmer_${version}_netcoreapp31.zip"
Compress-Archive -Path .\Publish\net48\* -DestinationPath ".\Publish\RadeonSoftwareSlimmer_${version}_net48.zip"

Write-Host "Published: $version"
Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');