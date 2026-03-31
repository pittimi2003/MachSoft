# Arquitectura
- Capa 1: `MachSoft.DesignSystem` (tokens/contrato).
- Capa 2: `MachSoft.UI` (wrappers `Mx*`, integración MudBlazor interna).
- Capa 3: Host apps (`Showcase`, `Template.Server`, `Template.Wasm`) consumiendo `Mx*`.
- Capa 4: tests.

## Dependencias
`DesignSystem <- UI <- Hosts <- Tests`
