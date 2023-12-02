# Run the tests
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Nothing special so far
# Add code coverage later
# Ensure build fails if tests fail

# Run the tests in the test project
# This will automatically do both .net core and .net framework versions
Write-Output '***** Testing solution...'
dotnet test .\test\RadeonSoftwareSlimmer.Test\RadeonSoftwareSlimmer.Test.csproj