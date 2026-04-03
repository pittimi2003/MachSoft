# Consumo

1. Referenciar paquete `MachSoft.UI`.
2. En `Program.cs`: `builder.Services.AddMachSoftUi();`
3. En `App.razor`: envolver rutas con `<MxThemeProvider>`.
4. En páginas de negocio usar solo componentes `Mx*`.

## Nota de validación operativa
Para validar template + showcase end-to-end:
- instalar browsers Playwright (`./build/install-playwright.sh`),
- ejecutar smoke visual (`./build/smoke-tests.sh`).

El smoke visual levanta los tres hosts automáticamente y deja screenshots en `artifacts/screenshots`.


## Shell oficial de workspace
- Usar `MxWorkspaceLayout` como layout raíz reusable para navegación + contenido.
- Contrato y matriz de comportamiento: `docs/components/MxWorkspaceLayout.md`.
- `Showcase` y `Template.*` consumen este contrato sin lógica de layout duplicada en host.
