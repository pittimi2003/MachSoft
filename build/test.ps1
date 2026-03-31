$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet test MachSoft.UiPlatform.sln -c Release --no-build --logger trx --results-directory artifacts/test-results
