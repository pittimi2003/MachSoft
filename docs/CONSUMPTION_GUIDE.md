# Consumo

1. Referenciar paquete `MachSoft.UI`.
2. En `Program.cs`: `builder.Services.AddMachSoftUi();`
3. En `App.razor`: envolver rutas con `<MxThemeProvider>`.
4. En páginas de negocio usar solo componentes `Mx*`.

## Nota de validación operativa
Para validar template + showcase end-to-end:
- levantar `MachSoft.UI.Showcase` (`./build/run-showcase.sh`),
- levantar plantilla server (`./build/run-template-server.sh`),
- levantar plantilla wasm (`./build/run-template-wasm.sh`),
- ejecutar smoke tests Playwright (requiere browsers instalados y que el test no esté marcado con `Skip`).
