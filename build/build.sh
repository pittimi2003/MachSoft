#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet build MachSoft.UiPlatform.sln -c Release --no-restore
