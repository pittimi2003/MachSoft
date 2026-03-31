# UI Foundation Contract (Mx*)

## Componentes base obligatorios
- `MxButton`
- `MxIconButton`
- `MxTextField`
- `MxSelect`
- `MxCheckbox`
- `MxDialog`
- `MxTooltip`
- `MxPageContainer`
- `MxSectionCard`

## Reglas de API pública
1. Apps de negocio consumen **solo `Mx*`**.
2. No exponer tipos `Mud*` en API pública salvo ADR explícita.
3. Wrappers deben mapearse a tokens oficiales, no a defaults visuales de MudBlazor.
4. Cualquier nuevo wrapper requiere:
   - test bUnit,
   - página/update de Showcase,
   - actualización de contrato/documentación.

## Reglas visuales contractuales
- Color primario institucional: `#005098`.
- Radios base: 4px / 8px (16px excepcional).
- Stroke base: 1px / 2px.
- Spacing: escala 4px-based.
- Focus visible obligatorio.
- Superficies técnicas (planas o plano-elevadas) con borde claro.

## Anti-reglas
- Prohibido introducir atajos visuales en host apps fuera de tokens/theme/librería.
- Prohibido usar MudBlazor como identidad visual final.
- Prohibido look Material genérico como resultado final.
