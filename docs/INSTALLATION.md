# Instalación

## Requisitos
- .NET 8 SDK (`8.0.100+`, ver `global.json`).
- Acceso a un feed NuGet alcanzable para restaurar paquetes (por defecto `https://api.nuget.org/v3/index.json`).
- Node no requerido para smoke tests; Playwright se instala desde el paquete .NET.
- Si el instalador genera `playwright.ps1` (sin `playwright.sh`), se requiere `pwsh` en Linux para completar instalación de browsers.

## Inicio rápido (orden operativo)
1. `./build/install.sh`
2. `./build/restore.sh`
3. `./build/build.sh`
4. `./build/test.sh`
5. `./build/install-playwright.sh`

## Diagnóstico rápido de conectividad NuGet
Si `restore` falla con `NU1301`, validar conectividad al feed configurado. Ejemplo:

- `curl -I https://api.nuget.org/v3/index.json`

Si el entorno requiere proxy o feed interno, configurar `NuGet.config` del entorno antes de continuar.
