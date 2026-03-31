#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet test tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj -c Debug --logger "trx;LogFileName=smoke-tests.trx" --results-directory artifacts/test-results
