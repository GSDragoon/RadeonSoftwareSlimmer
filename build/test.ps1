$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


$artifactPath = '.\artifacts'
$resultsPath  = '.\artifacts\tests'


Write-Output '***** Installing Report Generator...'
dotnet tool install dotnet-reportgenerator-globaltool --tool-path $artifactPath


Write-Output '***** Testing solution...'
# https://github.com/dotnet/sdk/issues/44991 - does not support artifact output
$project = '.\test\RadeonSoftwareSlimmer.Test\RadeonSoftwareSlimmer.Test.csproj'

dotnet test $project --no-build --configuration Release --results-directory $resultsPath --framework net9.0-windows
dotnet test $project --no-build --configuration Release --results-directory $resultsPath --framework net8.0-windows
dotnet test $project --no-build --configuration Release --results-directory $resultsPath --framework net48
Write-Output '***** Done Testing solution...'


Write-Output '***** Running Report Generator...'
$reportGenArgs = @(
  "-reports:${resultsPath}\*\coverage.cobertura*.xml",
  "-targetdir:${artifactPath}\CoverageReports",
  '-reporttypes:Badges;Cobertura;Html;HtmlSummary;TextSummary',
  #'-verbosity:Verbose',
  '--settings:createSubdirectoryForAllReportTypes=true'
)
Start-Process -FilePath "${artifactPath}\reportgenerator.exe" -ArgumentList $reportGenArgs  -NoNewWindow -Wait
Get-Content -Path "${artifactPath}\CoverageReports\TextSummary\Summary.txt"
