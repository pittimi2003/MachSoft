#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj"
FRAMEWORK="net8.0"
CONFIGURATION="Debug"

# Microsoft.Playwright expone playwright.sh tras compilar el proyecto de tests.
dotnet build "$PROJECT" -c "$CONFIGURATION"
PLAYWRIGHT_SH="tests/MachSoft.UI.Showcase.SmokeTests/bin/$CONFIGURATION/$FRAMEWORK/playwright.sh"

if [ ! -f "$PLAYWRIGHT_SH" ]; then
  echo "No se encontró $PLAYWRIGHT_SH. Verifica restore/build del proyecto de smoke tests." >&2
  exit 1
fi

"$PLAYWRIGHT_SH" install --with-deps
