#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet pack src/MachSoft.DesignSystem/MachSoft.DesignSystem.csproj -c Release --no-build && dotnet pack src/MachSoft.UI/MachSoft.UI.csproj -c Release --no-build
