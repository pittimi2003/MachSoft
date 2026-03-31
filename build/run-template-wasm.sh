#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet run --project src/MachSoft.Template.Wasm/MachSoft.Template.Wasm.csproj
