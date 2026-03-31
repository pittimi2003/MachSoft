$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet run --project src/MachSoft.UI.Showcase/MachSoft.UI.Showcase.csproj
