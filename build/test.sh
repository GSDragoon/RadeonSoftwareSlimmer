#!/bin/bash

set -eu


artifactPath='./artifacts'
resultsPath='./artifacts/tests'


echo '***** Installing Report Generator...'
dotnet tool install dotnet-reportgenerator-globaltool --tool-path $artifactPath


echo '***** Testing solution...'
# https://github.com/dotnet/sdk/issues/44991 - does not support artifact output
project='./test/RadeonSoftwareSlimmer.Test/RadeonSoftwareSlimmer.Test.csproj'

dotnet test $project --no-build --configuration Release --results-directory $resultsPath --framework net9.0-windows
dotnet test $project --no-build --configuration Release --results-directory $resultsPath --framework net8.0-windows
#dotnet test $project --no-build --configuration Release --results-directory $resultsPath --framework net48
echo '***** Done Testing solution...'


echo '***** Running Report Generator...'
#$reportGenArgs = @(
#  "-reports:${resultsPath}\*\coverage.cobertura*.xml",
#  "-targetdir:${artifactPath}\CoverageReports",
#  '-reporttypes:Badges;Cobertura;Html;HtmlSummary;TextSummary',
#  '--settings:createSubdirectoryForAllReportTypes=true'
#)
#Start-Process -FilePath "${artifactPath}\reportgenerator.exe" -ArgumentList $reportGenArgs  -NoNewWindow -Wait
#Get-Content -Path "${artifactPath}\CoverageReports\TextSummary\Summary.txt"
