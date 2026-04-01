# UI Foundation Contract (MachSoft `Mx*`)

## Componentes base obligatorios
### Catálogo aceptado Etapa 1 + Bloque 1
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

### Bloque 2 (inputs avanzados, feedback y data)
- `MxAutocomplete`: selección única con filtrado básico de opciones.
- `MxMultiSelect`: selección múltiple con estado seleccionado y deselección.
- `MxDatePicker`: captura de fecha única con formato visible.
- `MxDateRangePicker`: captura de rango inicio/fin.
- `MxFileUpload`: carga de archivo único con estados idle, seleccionado, disabled y error.
- `MxSnackbar`: notificación toast con tonos info/success/warning/error.
- `MxConfirmDialog`: diálogo de confirmación con acciones confirm/cancel.
- `MxTable`: tabla base para CRUD con columnas, filas, estado vacío y fila seleccionable.
- `MxPaginator`: paginación base con página actual y navegación previous/next.

## Alcance funcional mínimo Bloque 2
1. Soporte light/dark usando tokens del sistema actual (sin reabrir identidad base).
2. API pública sólo `Mx*` para apps de negocio.
3. Showcase con casos de uso operativos (inputs avanzados, feedback y listing).
4. Validación mínima obligatoria: clean/build/tests + smoke básico de showcase.

## Fuera de alcance por ahora
- Mega-grid con sorting, agrupación, virtualización y edición inline compleja.
- Upload multiarchivo con chunking/retry.
- Motor de validaciones cruzadas complejo o máscaras regionales avanzadas.
- Notificaciones persistentes con historial.

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
- Bloque 2 reutiliza tokens vigentes y sólo extiende estilos de componentes nuevos.

## Anti-reglas
- Prohibido atajo visual en host apps fuera de tokens/theme/librería.
- Prohibido look Material genérico como resultado final.
- Prohibido usar MudBlazor como autoridad visual.
