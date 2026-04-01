# UI Foundation Contract (MachSoft `Mx*`)

## Catálogo oficial gobernado

### 1) Foundation / Layout / Navigation
`MxThemeProvider`, `MxPageContainer`, `MxSectionCard`, `MxAppShell`, `MxAppBar`, `MxDrawer`, `MxNavMenu`, `MxNavGroup`, `MxBreadcrumb`, `MxTabs`, `MxAccordion`, `MxDivider`, `MxSpacer`, `MxMenu`, `MxPopover`, `MxTooltip`.

### 2) Inputs / Forms
`MxButton`, `MxIconButton`, `MxTextField`, `MxTextArea`, `MxSelect`, `MxAutocomplete`, `MxMultiSelect`, `MxCheckbox`, `MxRadio`, `MxRadioGroup`, `MxSwitch`, `MxDatePicker`, `MxDateRangePicker`, `MxTimePicker`, `MxNumberField`, `MxFileUpload`, `MxForm`, `MxFormSection`, `MxValidationSummary`.

### 3) Data / Listing / Collections
`MxDataGrid`, `MxPaginator`, `MxList`, `MxListItem`, `MxCard`, `MxChip`, `MxBadge`, `MxAvatar`, `MxTag`, `MxEmptyState`, `MxLoadingState`, `MxErrorState`.

### 4) Feedback / Overlay / Utility
`MxDialog`, `MxConfirmDialog`, `MxAlert`, `MxSnackbar`, `MxToast`, `MxProgress`, `MxSkeleton`, `MxBusyOverlay`.

### 5) Productivity / Patterns
`MxCrudToolbar`, `MxFilterBar`, `MxEntityHeader`, `MxSearchBox` y patrones de referencia:
- `MxDataGrid + MxPaginator + filtros + acciones`
- formulario operativo (`MxForm` + `MxFormSection` + validación)
- confirmación (`MxConfirmDialog`)
- estados (`MxEmptyState`, `MxLoadingState`, `MxErrorState`)

## Decisiones de API
1. Contrato público obligatorio: sólo `Mx*`.
2. `Mud*` queda como implementación interna encapsulada.
3. APIs de selección, validación y estados siguen naming consistente (`Value/ValueChanged`, `Error/ErrorText`, `Disabled`).
4. `MxDataGrid` es el control oficial de listing enterprise; `MxTable` queda sólo legado.
5. `MxToast` se unifica sobre infraestructura de `MxSnackbar` para evitar doble canal de notificaciones.

## Alcance soportado actual
- Light/dark coherentes con tokens MachSoft.
- Estados de interacción (normal/hover/focus/error/disabled) en controles de entrada.
- Flujos reales en showcase para navegación, formularios, listing, feedback y patterns.
- Integración de templates Server/Wasm con consumo `Mx*`.

## Límites actuales (deuda real)
- `MxDataGrid` aún no incorpora sorting server-side, agrupación y virtualización.
- `MxFileUpload` soporta drag-and-drop visual/interacción y selección; no incorpora chunk upload/retry.
- `MxAutocomplete` soporta búsqueda local; motor remoto queda a integración de aplicación.

## Reglas de consistencia
1. No exponer tipos `Mud*` en APIs de negocio.
2. No hardcodear identidad visual fuera de tokens/UI package.
3. Nuevo/ajuste de control exige: test bUnit + showcase + actualización contractual.
4. `MachSoft.DesignSystem` es la autoridad de tokens.

## Relación con Showcase y Templates
- Showcase es el entorno de evaluación contractual de estados y escenarios.
- Templates Server/Wasm validan uso real fuera del showcase sin divergencia de contrato.
