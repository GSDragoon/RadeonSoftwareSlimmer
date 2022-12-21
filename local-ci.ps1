# Local version of CI to run before committing changes

# Delete old directory, if it exists
if (Test-Path -Path .\local-ci)
{
	Remove-Item -Path .\local-ci -Recurse -Force
}

# Create new directory
New-Item -ItemType Directory -Path .\local-ci -Force | Out-Null

# Directories being used for results/output
$localCiDir = Resolve-Path -Path .\local-ci
$testResultsDir = "${localCiDir}\TestResults"
$coveragerDir = "${localCiDir}\CoverageReports"
$publishDir = "${localCiDir}\Publish"

# Install Report Generator
$reportGeneratorPath = "${localCiDir}\ReportGeneratorTool"
$reportGeneratorExe = "${reportGeneratorPath}\reportgenerator.exe"
dotnet tool install dotnet-reportgenerator-globaltool --tool-path "${reportGeneratorPath}"

# Clean
dotnet clean --configuration Release --framework net7.0-windows
dotnet clean --configuration Release --framework net6.0-windows
dotnet clean --configuration Release --framework net48

# Build
dotnet build --configuration Release --framework net7.0-windows
dotnet build --configuration Release --framework net6.0-windows
dotnet build --configuration Release --framework net48

# Test
dotnet test --no-build --configuration Release --results-directory $testResultsDir

# Run Report Generator to create coverage reports
$reportGenArgs = @(
  "-reports:${testResultsDir}\*\coverage.cobertura*.xml",
  "-targetdir:${coveragerDir}",
  "-reporttypes:Badges;Cobertura;Html;HtmlSummary;TextSummary",
  #"-verbosity:Verbose",
  "--settings:createSubdirectoryForAllReportTypes=true"
)
& $reportGeneratorExe $reportGenArgs

# Output coverage results to console
Get-Content "${coveragerDir}\TextSummary\Summary.txt"

# Build and create artifacts
dotnet publish --configuration Release --framework net7.0-windows --self-contained false --force --output "${publishDir}\net70" .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci
dotnet publish --configuration Release --framework net6.0-windows --self-contained false --force --output "${publishDir}\net60" .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci
dotnet publish --configuration Release  --framework net48 --force --output "${publishDir}\net48" .\src\RadeonSoftwareSlimmer\RadeonSoftwareSlimmer.csproj -p:VersionSuffix=local-ci

# Get the version from the published executable
$version = (Get-Item "${publishDir}\net70\RadeonSoftwareSlimmer.exe").VersionInfo.ProductVersion

# Archive the artifacts
Compress-Archive -Path "${publishDir}\net70\*" -DestinationPath "${publishDir}\RadeonSoftwareSlimmer_${version}_net70.zip"
Compress-Archive -Path "${publishDir}\net60\*" -DestinationPath "${publishDir}\RadeonSoftwareSlimmer_${version}_net60.zip"
Compress-Archive -Path "${publishDir}\net48\*" -DestinationPath "${publishDir}\RadeonSoftwareSlimmer_${version}_net48.zip"

# Output the version
Write-Host "Published: $version"

# Wait for keypress so the output can be read
Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');