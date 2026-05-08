#!/bin/bash

set -eu


export version='1.2.3-localci'

echo '***** Recreating artifacts directory...'
artifactPath='./artifacts'
resultsPath='./artifacts/tests'

echo $artifactPath
if [[ -d "${artifactPath}" ]]; then
  rm --recursive --force "${artifactPath:?}/*"
fi

project='./src/RadeonSoftwareSlimmer.Core/RadeonSoftwareSlimmer.Core.csproj'
testProject='./test/RadeonSoftwareSlimmer.Core.Test/RadeonSoftwareSlimmer.Core.Test.csproj'

echo '***** Building Core Project...'
dotnet build $project --no-incremental --force --configuration Release -p:Version=$version

echo '***** Building Core Test Project...'
dotnet build $testProject --no-incremental --force --configuration Release -p:Version=$version --framework net10.0
echo '***** Done Building'

echo '***** Testing...'
# https://github.com/dotnet/sdk/issues/44991 - does not support artifact output
dotnet test $testProject --no-build --configuration Release --results-directory $resultsPath --framework net10.0

echo '***** Done!'
