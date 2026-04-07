# MachSoft UI Platform

Plataforma base frontend Blazor (.NET 8) para estandarizar experiencia y acelerar nuevos proyectos.

## Principios
- API pública de UI: `Mx*` (no `Mud*` en negocio).
- `MachSoft.DesignSystem` gobierna tokens, escala y theme.
- `MachSoft.UI` encapsula MudBlazor como detalle interno.
- Las demos oficiales Server y WebAssembly prueban consumo real del contrato.

## Estructura
- `src/MachSoft.DesignSystem`: tokens + contrato de tema.
- `src/MachSoft.UI`: wrappers y composición base.
- `src/MachSoft.Template.Server|Wasm`: templates oficiales de arranque.
- `src/MachSoft.Demo.Server|WebAssembly`: demos ejecutables oficiales.
- `src/MachSoft.Template.Server|Wasm`: demos oficiales de arranque.
- `tests/*`: pruebas unitarias/componentes y smoke visual.
- `build/*`: scripts operativos.
- `docs/*`: documentación normativa y operativa.

## Bootstrap de nuevas apps
- Guía práctica: `docs/APP_BOOTSTRAP_GUIDE.md`.

## Higiene de artefactos
- Los artefactos generados por build/test/smoke (`bin/`, `obj/`, `.vs/`, `artifacts/`, `TestResults/`, `test-results/`, `coverage/`, `playwright-report/`) deben permanecer fuera de Git y se regeneran localmente.
