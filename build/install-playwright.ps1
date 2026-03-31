$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

$project = "tests/MachSoft.UI.Showcase.SmokeTests/MachSoft.UI.Showcase.SmokeTests.csproj"
$framework = "net8.0"
$configuration = "Debug"
$outputDir = "tests/MachSoft.UI.Showcase.SmokeTests/bin/$configuration/$framework"
$playwrightPs1 = Join-Path $outputDir "playwright.ps1"
$playwrightSh = Join-Path $outputDir "playwright.sh"
$playwrightCli = Join-Path $outputDir ".playwright/package/cli.js"

# Asegura generación de scripts/runtime de Playwright.
dotnet build $project -c $configuration | Out-Host

if (Test-Path $playwrightPs1) {
  & $playwrightPs1 install --with-deps
  exit $LASTEXITCODE
}

if (Test-Path $playwrightSh) {
  bash $playwrightSh install --with-deps
  exit $LASTEXITCODE
}

if (Test-Path $playwrightCli) {
  $nodeCandidates = @(
    (Join-Path $outputDir ".playwright/node/win32_x64/node.exe"),
    (Join-Path $outputDir ".playwright/node/win32_arm64/node.exe"),
    (Join-Path $outputDir ".playwright/node/linux-x64/node"),
    (Join-Path $outputDir ".playwright/node/linux-arm64/node"),
    (Join-Path $outputDir ".playwright/node/darwin-x64/node"),
    (Join-Path $outputDir ".playwright/node/darwin-arm64/node")
  )

  foreach ($nodeExecutable in $nodeCandidates) {
    if (Test-Path $nodeExecutable) {
      & $nodeExecutable $playwrightCli install --with-deps
      exit $LASTEXITCODE
    }
  }
}

throw "No se encontró ejecutor operativo de Playwright. Revisar $playwrightPs1 / $playwrightSh / $playwrightCli."
