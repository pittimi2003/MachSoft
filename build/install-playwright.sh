#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj"
FRAMEWORK="net8.0"
CONFIGURATION="Debug"

# Microsoft.Playwright expone un script de instalación en salida de build.
dotnet build "$PROJECT" -c "$CONFIGURATION"
PLAYWRIGHT_SH="tests/MachSoft.UI.Showcase.SmokeTests/bin/$CONFIGURATION/$FRAMEWORK/playwright.sh"
PLAYWRIGHT_PS1="tests/MachSoft.UI.Showcase.SmokeTests/bin/$CONFIGURATION/$FRAMEWORK/playwright.ps1"

if [ -f "$PLAYWRIGHT_SH" ]; then
  "$PLAYWRIGHT_SH" install --with-deps
  exit 0
fi

if [ -f "$PLAYWRIGHT_PS1" ]; then
  if command -v pwsh >/dev/null 2>&1; then
    pwsh "$PLAYWRIGHT_PS1" install --with-deps
    exit 0
  fi

  echo "Se encontró $PLAYWRIGHT_PS1 pero pwsh no está instalado en el entorno." >&2
  exit 1
fi

echo "No se encontró script de Playwright ($PLAYWRIGHT_SH ni $PLAYWRIGHT_PS1). Verifica restore/build del proyecto de smoke tests." >&2
exit 1
