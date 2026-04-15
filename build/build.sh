#!/bin/bash

set -eu


version=$BUILD_VERSION
echo "Version: ${version}"

echo '***** Building solution...'
dotnet build --no-incremental --force --configuration Release -p:Version=$version --framework net9.0-windows
dotnet build --no-incremental --force --configuration Release -p:Version=$version --framework net8.0-windows
#dotnet build --no-incremental --force --configuration Release -p:Version=$version --framework net48
echo '***** Done Building solution...'
