$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet tool restore && dotnet test tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj --filter FullyQualifiedName~SkipNever
