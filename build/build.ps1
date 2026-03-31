$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet build MachSoft.UiPlatform.sln -c Release --no-restore
