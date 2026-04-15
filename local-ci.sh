#!/bin/bash

set -eu


export BUILD_VERSION='1.2.3-localci'

echo '***** Recreating artifacts directory...'
artifactPath='./artifacts'
echo $artifactPath
if [[ -d "${artifactPath}" ]]; then
  rm --recursive --force "${artifactPath:?}/*"
fi


bash ./build/build.sh

bash ./build/test.sh

bash ./build/publish.sh


echo '***** Done!'
