# UI Foundation Contract (MachSoft `Mx*`)

## Catálogo oficial gobernado

### 1) Buttons
`MxButton`, `MxIconButton`.

Capacidades obligatorias:
- variantes: filled (primary), outlined (secondary), text (tertiary/link);
- icon-left, icon-right, icon-only;
- disabled, loading, active/pressed;
- tamaños small/medium/large;
- soporte light/dark coherente con tokens.

### 2) Forms
`MxTextField`, `MxTextArea`, `MxSelect`, `MxCheckbox`, `MxRadio`, `MxRadioGroup`, `MxSwitch`, `MxNumberField`, `MxTimePicker`, `MxDatePicker`, `MxDateRangePicker`, `MxForm`, `MxFormSection`, `MxValidationSummary`.

Capacidades obligatorias:
- estados normal/hover/focus/error/disabled;
- validación visual clara y mensajes consistentes (`Error/ErrorText`);
- densidad y layout compatibles con escenarios de negocio.

### 3) Advanced Inputs
`MxAutocomplete`, `MxMultiSelect`, `MxFileUpload`, `MxSearchBox`.

Capacidades obligatorias:
- `MxFileUpload` soporta drag-and-drop real sobre superficie, selección múltiple, lista/remoción y validación visual de tipo/tamaño;
- estados de `MxFileUpload`: idle, drag-over, selected/success, invalid/error, uploading/progress y disabled;
- `MxAutocomplete`/`MxMultiSelect` con patrones de selección de catálogo enterprise.

### 4) Navigation
`MxAppShell`, `MxAppBar`, `MxDrawer`, `MxNavMenu`, `MxNavGroup`, `MxBreadcrumb`, `MxTabs`, `MxAccordion`, `MxMenu`, `MxPopover`, `MxTooltip`, `MxDivider`, `MxSpacer`.

### 5) Dialogs
`MxDialog`, `MxConfirmDialog`.

### 6) Feedback
`MxAlert`, `MxSnackbar`, `MxToast`, `MxProgress`, `MxSkeleton`, `MxBusyOverlay`, `MxEmptyState`, `MxLoadingState`, `MxErrorState`.

### 7) DataGrid / Listing
`MxDataGrid`, `MxPaginator`, `MxList`, `MxListItem`, `MxCard`, `MxChip`, `MxBadge`, `MxAvatar`, `MxTag`.

Capacidades obligatorias de `MxDataGrid`:
- header fuerte, columnas reales y filas reales;
- hover + active + selección de fila;
- toolbar superior con búsqueda, filtros rápidos y bloque de filtros avanzados;
- ordenamiento por columna;
- integración real con `MxPaginator` usando totales de resultado filtrado;
- estados empty/loading/error;
- dataset de ejemplo realista en showcase;
- patrón CRUD operativo.

> `MxDataGrid` sustituye al enfoque de tabla básica para listados de negocio.

### 8) Patterns
`MxCrudToolbar`, `MxFilterBar`, `MxEntityHeader` + composición con `MxDataGrid`, `MxPaginator`, `MxConfirmDialog` y estados.

## Norma contractual: iconografía integrada en controles

Aplica a `MxButton`, `MxIconButton`, `MxMenu`, `MxTabs`, acciones de toolbar, acciones de grid e inputs con adornments.

Regla visual:
1. En estado normal, el icono acompaña al texto y no domina jerarquía.
2. El refuerzo visual principal de iconografía ocurre en hover/focus/active/selected/pressed.
3. No se permite iconografía “pegada” sin integración de espaciado, opacidad y alineación del control.
4. Excepción: acciones críticas (`Danger`) pueden elevar contraste de icono, manteniendo legibilidad del texto.

## Controles elevados en esta fase
- `MxDataGrid`: toolbar, quick/advanced filters, sorting, selección por página, paginación integrada y estados auditables.
- `MxFileUpload`: superficie de drop reforzada, drag-over distinguible, estado con archivos, remove, validación y progreso.
- `MxButton`/`MxIconButton`/`MxMenu`/`MxTabs`: normalización de integración iconográfica por estado.
- Showcase: matriz de botones exhaustiva y página de auditoría contractual del catálogo.

## Decisiones de API
1. Contrato público obligatorio: sólo `Mx*`.
2. `Mud*` queda como implementación interna encapsulada.
3. Naming consistente: `Value/ValueChanged`, `SelectedItems/SelectedItemsChanged`, `Error/ErrorText`, `Disabled`, `Loading`.
4. `MxDataGrid` concentra capacidades de listing enterprise (filtros/ordenamiento/estados) y reemplaza una tabla básica.
5. `MxFileUpload` deja de ser wrapper superficial y queda como control documental con drag-and-drop operativo.

## Showcase contractual
La Showcase oficial se considera cerrada cuando permite revisión visual control por control y variante por variante:
- familias completas: Buttons, Forms, Advanced Inputs, Navigation, Dialogs, Feedback, DataGrid/Listing, States, Patterns;
- estados relevantes: normal/hover/focus/selected/error/disabled/loading;
- tamaños y variantes donde aplique;
- escenarios de uso reales y auditables en light/dark;
- matriz de auditoría visible con cobertura por familia/control/estado.

## Límites actuales (deuda real abierta)
- `MxDataGrid` no incorpora virtualización ni agrupación server-side.
- `MxFileUpload` no incorpora chunk upload/retry ni persistencia de progreso backend.

## Reglas de consistencia
1. No exponer tipos `Mud*` en APIs de negocio.
2. No hardcodear identidad visual fuera de tokens/UI package.
3. Nuevo/ajuste de control exige: test bUnit + showcase + actualización contractual.
4. `MachSoft.DesignSystem` es la autoridad de tokens.
