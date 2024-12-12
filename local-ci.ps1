# Local version of CI to run before committing changes
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


Write-Output '***** Recreating local-ci directory...'
if (Test-Path -Path .\local-ci) {
	Remove-Item -Path .\local-ci -Recurse -Force
}

New-Item -ItemType Directory -Path .\local-ci -Force | Out-Null

$localCiDir = Resolve-Path -Path .\local-ci
$testResultsDir = "${localCiDir}\TestResults"
$coveragerDir = "${localCiDir}\CoverageReports"
$publishDir = "${localCiDir}\Publish"


Write-Output '***** Installing Report Generator...'
$reportGeneratorExe = "${localCiDir}\reportgenerator.exe"
dotnet tool install dotnet-reportgenerator-globaltool --tool-path "${localCiDir}"


Write-Output '***** Cleaning solution...'
dotnet clean --verbosity minimal --configuration Release --framework net9.0-windows
dotnet clean --verbosity minimal --configuration Release --framework net8.0-windows
dotnet clean --verbosity minimal --configuration Release --framework net48


Write-Output '***** Building solution...'
dotnet build --configuration Release --framework net9.0-windows
dotnet build --configuration Release --framework net8.0-windows
dotnet build --configuration Release --framework net48


Write-Output '***** Testing solution...'
dotnet test --no-build --configuration Release --framework net9.0-windows --results-directory "${testResultsDir}"
dotnet test --no-build --configuration Release --framework net8.0-windows --results-directory "${testResultsDir}"
dotnet test --no-build --configuration Release --framework net48 --results-directory "${testResultsDir}"


Write-Output '***** Running Report Generator...'
$reportGenArgs = @(
  "-reports:${testResultsDir}\*\coverage.cobertura*.xml",
  "-targetdir:${coveragerDir}",
  '-reporttypes:Badges;Cobertura;Html;HtmlSummary;TextSummary',
  #'-verbosity:Verbose',
  '--settings:createSubdirectoryForAllReportTypes=true'
)
& $reportGeneratorExe $reportGenArgs

Get-Content "${coveragerDir}\TextSummary\Summary.txt"


Write-Output '***** Publishing solution...'
dotnet publish --configuration Release --framework net9.0-windows --force --output "${publishDir}\net90" .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci
dotnet publish --configuration Release --framework net8.0-windows --force --output "${publishDir}\net80" .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci
dotnet publish --configuration Release  --framework net48 --force --output "${publishDir}\net48" .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci

$version = (Get-Item -Path "${publishDir}\net90\RadeonSoftwareSlimmer.exe").VersionInfo.ProductVersion


Write-Output '***** Archiving artifacts...'
Compress-Archive -Path "${publishDir}\net90\*" -DestinationPath "${publishDir}\RadeonSoftwareSlimmer_${version}_net90.zip"
Compress-Archive -Path "${publishDir}\net80\*" -DestinationPath "${publishDir}\RadeonSoftwareSlimmer_${version}_net80.zip"
Compress-Archive -Path "${publishDir}\net48\*" -DestinationPath "${publishDir}\RadeonSoftwareSlimmer_${version}_net48.zip"


Write-Output "Published: $version"
# Wait for keypress so the output can be read
Write-Output '***** Done! Press any key to continue...'
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');