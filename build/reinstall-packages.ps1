$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet nuget locals all --clear && dotnet restore MachSoft.UiPlatform.sln
