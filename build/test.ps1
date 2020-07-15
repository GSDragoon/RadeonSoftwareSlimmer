# Run the tests

# Nothing special so far
# Add code coverage later
# Ensure build fails if tests fail

# Run the tests in the test project
# This will automatically do both .net core and .net framework versions
dotnet test .\test\RadeonSoftwareSlimmer.Test\RadeonSoftwareSlimmer.Test.csproj


$param1=$args[0]
Write-Host "Parameter: $param1"

Write-Host "GitHub Value: ${{ steps.gitversion.outputs.semVer }}"

Write-Host "Environmental Variable: $Env:steps.gitversion.outputs.semVer"