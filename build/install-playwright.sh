#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj"
FRAMEWORK="net8.0"
CONFIGURATION="Debug"
OUTPUT_DIR="tests/MachSoft.UI.Showcase.SmokeTests/bin/$CONFIGURATION/$FRAMEWORK"
PLAYWRIGHT_SH="$OUTPUT_DIR/playwright.sh"
PLAYWRIGHT_PS1="$OUTPUT_DIR/playwright.ps1"
PLAYWRIGHT_NODE_DIR="$OUTPUT_DIR/.playwright/node"
PLAYWRIGHT_PACKAGE_CLI="$OUTPUT_DIR/.playwright/package/cli.js"

# Asegura generación de scripts/runtime de Playwright.
dotnet build "$PROJECT" -c "$CONFIGURATION"

if [ -f "$PLAYWRIGHT_SH" ]; then
  "$PLAYWRIGHT_SH" install --with-deps
  exit 0
fi

if [ -f "$PLAYWRIGHT_PS1" ] && command -v pwsh >/dev/null 2>&1; then
  pwsh "$PLAYWRIGHT_PS1" install --with-deps
  exit 0
fi

if [ -f "$PLAYWRIGHT_PACKAGE_CLI" ] && [ -d "$PLAYWRIGHT_NODE_DIR" ]; then
  if [ -x "$PLAYWRIGHT_NODE_DIR/linux-x64/node" ]; then
    "$PLAYWRIGHT_NODE_DIR/linux-x64/node" "$PLAYWRIGHT_PACKAGE_CLI" install --with-deps
    exit 0
  fi

  if [ -x "$PLAYWRIGHT_NODE_DIR/linux-arm64/node" ]; then
    "$PLAYWRIGHT_NODE_DIR/linux-arm64/node" "$PLAYWRIGHT_PACKAGE_CLI" install --with-deps
    exit 0
  fi

  if [ -x "$PLAYWRIGHT_NODE_DIR/darwin-x64/node" ]; then
    "$PLAYWRIGHT_NODE_DIR/darwin-x64/node" "$PLAYWRIGHT_PACKAGE_CLI" install --with-deps
    exit 0
  fi

  if [ -x "$PLAYWRIGHT_NODE_DIR/darwin-arm64/node" ]; then
    "$PLAYWRIGHT_NODE_DIR/darwin-arm64/node" "$PLAYWRIGHT_PACKAGE_CLI" install --with-deps
    exit 0
  fi

  if [ -x "$PLAYWRIGHT_NODE_DIR/win32_x64/node.exe" ]; then
    "$PLAYWRIGHT_NODE_DIR/win32_x64/node.exe" "$PLAYWRIGHT_PACKAGE_CLI" install --with-deps
    exit 0
  fi
fi

echo "No se encontró un ejecutor operativo de Playwright. Revisa $PLAYWRIGHT_SH, $PLAYWRIGHT_PS1 y $PLAYWRIGHT_PACKAGE_CLI." >&2
exit 1
