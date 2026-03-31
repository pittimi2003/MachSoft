$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
dotnet pack src/MachSoft.DesignSystem/MachSoft.DesignSystem.csproj -c Release --no-build && dotnet pack src/MachSoft.UI/MachSoft.UI.csproj -c Release --no-build
