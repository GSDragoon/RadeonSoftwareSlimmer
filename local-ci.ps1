$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'


$Env:BUILD_VERSION = '1.2.3-localci'

Write-Output '***** Recreating artifacts directory...'
$artifactPath = '.\artifacts'
if (Test-Path -Path $artifactPath) {
	Remove-Item -Path $artifactPath -Recurse -Force
}


& .\build\build.ps1

& .\build\test.ps1

& .\build\publish.ps1


Write-Output '***** Done! Press any key to continue...'
Read-Host | Out-Null
