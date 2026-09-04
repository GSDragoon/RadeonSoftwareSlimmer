$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


$artifactPath = '.\artifacts'
$resultsPath  = '.\artifacts\tests'


Write-Output '***** Installing Report Generator...'
dotnet tool install dotnet-reportgenerator-globaltool --tool-path $artifactPath


Write-Output '***** Testing solution...'
# https://github.com/dotnet/sdk/issues/44991 - does not support artifact output
$coreProject    = '.\test\RadeonSoftwareSlimmer.Core.Test\RadeonSoftwareSlimmer.Core.Test.csproj'
$windowsProject = '.\test\RadeonSoftwareSlimmer.Windows.Test\RadeonSoftwareSlimmer.Windows.Test.csproj'

dotnet test $coreProject    --no-build --configuration Release --results-directory $resultsPath --framework net10.0
dotnet test $coreProject    --no-build --configuration Release --results-directory $resultsPath --framework net48

dotnet test $windowsProject --no-build --configuration Release --results-directory $resultsPath --framework net10.0-windows
dotnet test $windowsProject --no-build --configuration Release --results-directory $resultsPath --framework net48
Write-Output '***** Done Testing solution...'


Write-Output '***** Running Report Generator...'
$reportGenArgs = @(
  "-reports:${resultsPath}\*\coverage.cobertura*.xml",
  "-targetdir:${artifactPath}\CoverageReports",
  '-reporttypes:Badges;Cobertura;Html;HtmlSummary;MarkdownSummaryGithub;TextSummary',
  '--settings:createSubdirectoryForAllReportTypes=true'
)
Start-Process -FilePath "${artifactPath}\reportgenerator.exe" -ArgumentList $reportGenArgs  -NoNewWindow -Wait
Get-Content -Path "${artifactPath}\CoverageReports\TextSummary\Summary.txt"
