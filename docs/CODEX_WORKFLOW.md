# Flujo Codex

1. Implementar historia en `MachSoft.UI` o template.
2. Asegurar uso de `Mx*`.
3. Actualizar showcase.
4. Añadir/ajustar pruebas.
5. Ejecutar validación técnica en este orden:
   - `restore`
   - `build`
   - `test`
   - instalación Playwright (`./build/install-playwright.sh`)
   - smoke tests reales (`./build/smoke-tests.sh`)
   - `pack`
6. Documentar cambios en ADR si altera contratos.

## Precondición operativa para Playwright
- `build/install-playwright.sh` intenta primero `playwright.sh`.
- Si solo existe `playwright.ps1`, usa `pwsh` cuando está disponible.
- Si no hay `pwsh`, usa fallback operativo con Node embebido (`.playwright/node/.../node`) ejecutando `cli.js install --with-deps`.

## Criterio de cierre en validación real
El smoke test real (`MachSoft.UI.Showcase.SmokeTests`) queda cerrado solo si:
- no usa `Skip`,
- levanta hosts reales,
- valida rutas mínimas de showcase y templates,
- genera screenshots en `artifacts/screenshots`.

Si un paso queda bloqueado (por ejemplo `NU1301` por conectividad a feed NuGet), dejar diagnóstico explícito con:
- causa exacta,
- archivos afectados,
- corrección propuesta,
- impacto para inicio de historias de usuario.
