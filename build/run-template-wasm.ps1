$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet run --project src/MachSoft.Template.Wasm/MachSoft.Template.Wasm.csproj
