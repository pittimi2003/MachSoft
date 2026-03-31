$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet run --project src/MachSoft.Template.Server/MachSoft.Template.Server.csproj
