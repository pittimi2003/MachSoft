# Instalación

## Requisitos
- .NET 8 SDK (`8.0.100+`, ver `global.json`).
- Acceso a un feed NuGet alcanzable para restaurar paquetes (por defecto `https://api.nuget.org/v3/index.json`).
- Node no requerido para smoke tests; Playwright usa el runtime embebido en el paquete .NET.

## Inicio rápido (orden operativo)
1. `./build/install.sh`
2. `./build/restore.sh`
3. `./build/build.sh`
4. `./build/test.sh`
5. `./build/install-playwright.sh`
6. `./build/smoke-tests.sh`

## Playwright en entornos Linux sin `pwsh`
`build/install-playwright.sh` usa este orden real:
1. `playwright.sh` (si existe),
2. `playwright.ps1` con `pwsh` (si existe `pwsh`),
3. fallback al runtime Node embebido del paquete `.playwright/node/*/node` + `.playwright/package/cli.js`.

Con esto la instalación de browsers no depende de tener `pwsh` en Linux.

## Smoke tests visuales
`build/smoke-tests.sh`:
- levanta automáticamente `Showcase`, `Template Server` y `Template Wasm` en puertos locales fijos para la corrida;
- valida rutas clave;
- guarda screenshots en `artifacts/screenshots`.

## Diagnóstico rápido de conectividad NuGet
Si `restore` falla con `NU1301`, validar conectividad al feed configurado. Ejemplo:

- `curl -I https://api.nuget.org/v3/index.json`

Si el entorno requiere proxy o feed interno, configurar `NuGet.config` del entorno antes de continuar.
