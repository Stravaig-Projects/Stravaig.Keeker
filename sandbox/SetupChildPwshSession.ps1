$module = 'Stravaig.Keeker.PowerShellModule'

dotnet build $PSScriptRoot/../src/$module/$module.csproj -o $PSScriptRoot/$module
if ($LastExitCode -ne 0)
{
    Write-Output "Failed, exiting child pwsh session."
    Exit 1;
}

try
{
    Import-Module "$PSScriptRoot/$module/$module.dll" -ErrorAction Stop
}
catch
{
    Write-Error $_
    Write-Output "Failed, exiting child pwsh session."
    Exit 2;
}