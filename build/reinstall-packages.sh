#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet nuget locals all --clear && dotnet restore MachSoft.UiPlatform.sln
