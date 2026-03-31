#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet tool restore && dotnet test tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj --filter FullyQualifiedName~SkipNever
