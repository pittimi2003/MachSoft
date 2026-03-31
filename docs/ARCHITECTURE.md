# Arquitectura de UI Platform (MachSoft)

## Capas
1. **MachSoft.DesignSystem**
   - Fuente única de tokens (color, contenido, superficies, bordes, foco, spacing, radius, stroke, elevación, tipografía).
   - Contrato de tema (`ThemeContract`) y catálogo oficial (`ThemeCatalog.Light`, `ThemeCatalog.Dark`).

2. **MachSoft.UI**
   - API pública de componentes `Mx*`.
   - Bridge de implementación (MudBlazor) encapsulado por `MxThemeFactory`, `MxThemeProvider` y CSS de sistema.
   - Prohibido usar defaults Material como identidad final.

3. **Hosts** (`MachSoft.UI.Showcase`, templates)
   - Consumen `Mx*` exclusivamente.
   - No definen identidad visual propia fuera del contrato.
   - Implementan switch light/dark reutilizando el mismo tema oficial.

4. **Tests**
   - bUnit para wrappers `Mx*`.
   - smoke tests para render base del Showcase.

## Dirección visual oficial implementada
- Base estructural: familia Graphite.
- Brand accent principal: cian controlado (`#00B5D8`).
- Azul profundo secundario: soporte institucional (`#123E73`).
- Personalidad: enterprise técnica, sobria, precisa.

## Flujo de gobernanza
1. Cambios de identidad visual primero en `MachSoft.DesignSystem`.
2. Luego mapeo técnico en `MachSoft.UI`.
3. Luego adopción en Showcase/Templates.
4. Verificación con tests y actualización documental contractual.

## Dependencias
`MachSoft.DesignSystem <- MachSoft.UI <- Hosts <- Tests`

## Reglas técnicas obligatorias
- No exponer `Mud*` en API pública de negocio sin ADR.
- No hardcodear identidad en apps host.
- Todo cambio visual reusable debe entrar por tokens/theme/componentes `Mx*`.
