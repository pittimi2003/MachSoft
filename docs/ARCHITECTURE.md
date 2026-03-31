# Arquitectura de UI Platform (MachSoft)

## Capas
1. **MachSoft.DesignSystem**
   - Fuente única de tokens (color, escala, shape, elevación, tipografía).
   - Contrato de tema (`ThemeContract`) y catálogo light/dark (`ThemeCatalog`).

2. **MachSoft.UI**
   - Wrappers públicos `Mx*`.
   - Integración de MudBlazor encapsulada (`MxThemeFactory`, estilos CSS de sistema).
   - Nunca expone MudBlazor como contrato de negocio.

3. **Host apps** (`MachSoft.UI.Showcase`, templates)
   - Consumen `Mx*` exclusivamente.
   - Demuestran uso y consistencia del sistema.

4. **Tests**
   - bUnit para wrappers y smoke tests para Showcase.

## Dirección visual obligatoria
- Primary institucional: `#005098`.
- Acento técnico: cian de soporte, no dominante.
- Estética enterprise técnica, sobria, robusta.

## Dependencias
`DesignSystem <- UI <- Hosts <- Tests`

## Reglas de gobernanza técnica
- Cambios visuales reutilizables se introducen primero en `MachSoft.DesignSystem`.
- `MachSoft.UI` aplica los tokens al runtime (Mud theme + CSS system).
- Hosts no definen identidad visual propia por fuera de tokens.
