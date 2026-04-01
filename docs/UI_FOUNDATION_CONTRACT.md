# UI Foundation Contract (MachSoft `Mx*`)

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
- `MxTextArea`
- `MxRadio`
- `MxRadioGroup`
- `MxSwitch`
- `MxAlert`
- `MxProgress`
- `MxSkeleton`
- `MxMenu`
- `MxTabs`

## Reglas de API pública
1. Las apps de negocio consumen **solo `Mx*`**.
2. No exponer tipos `Mud*` en API pública salvo ADR aprobada.
3. Wrappers `Mx*` deben mapearse a tokens/theme MachSoft, no a defaults Material.
4. Nuevo componente `Mx*` exige:
   - test bUnit,
   - showcase actualizado,
   - documentación contractual actualizada.

## Contrato visual obligatorio
- Accent principal: `#00B5D8` (hover `#0099BA`, active `#007F99`).
- Base neutral: familia Graphite.
- Azul profundo secundario: `#123E73` (uso de apoyo).
- Radios operativos: `4px` y `8px`.
- Stroke base: `1px` y `2px`.
- Spacing: 4px-based.
- Focus visible obligatorio.
- Superficies técnicas planas/plano-elevadas con bordes claros.

## Integración de tema
- `MxThemeProvider` es el entrypoint para light/dark.
- `MxThemeFactory` traduce `ThemeCatalog` a `MudTheme` sin exponer identidad MudBlazor.

## Anti-reglas
- Prohibido atajo visual en host apps fuera de tokens/theme/librería.
- Prohibido look Material genérico como resultado final.
- Prohibido usar MudBlazor como autoridad visual.
