#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet test MachSoft.UiPlatform.sln -c Release --logger trx --results-directory artifacts/test-results
