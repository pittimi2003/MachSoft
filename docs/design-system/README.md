# Machsoft Design System Playbook

Este playbook convierte el design system en una plataforma de producto (no sólo una librería de estilos).

## 1) Cobertura de componentes

### Componentes disponibles hoy
- `MxDesignSystemProvider`
- `MxMainContainer`
- `MxButton`
- `MxIconButton`
- `MxAlert`
- `MxAppBar`
- `MxTextField`
- `MxSelect`
- `MxCheckbox`
- `MxSwitch`
- `MxRadioGroup`
- `MxTooltip`
- `MxCard`
- `MxSimpleTable`
- `MxSnackbarButton`
- `MxConfirmDialogButton`
- `MxDataGrid`
- `MxDatePicker`
- `MxDateRangePicker`
- `MxTimePicker`
- `MxPagination`
- `MxSkeleton`
- `MxFileUpload`
- `MxDropZone`
- `MxDrawer`
- `MxNavMenu`
- `MxTabs`
- `MxBreadcrumbs`
- `MxMenu`
- `MxTreeView`
- `MxSplitPanel`
- `MxSwipeArea`
- `MxHotkey`
- `MxChatBubble`
- `MxTimeline`
- `MxCharts`
- `MxCarousel`
- `MxAutocomplete`
- `MxAvatar`
- `MxBadge`
- `MxChip`
- `MxList`
- `MxProgress`
- `MxPaper`
- `MxStack`
- `MxSpacer`
- `MxDivider`
- `MxOverlay`
- `MxNumericField`
- `MxSlider`
- `MxRating`
- `MxContainer`
- `MxForm`
- `MxExpansionPanel`
- `MxLink`
- `MxImage`
- `MxChipSet`
- `MxCollapse`
- `MxGrid`
- `MxHidden`
- `MxPopover`
- `MxToggleGroup`
- `MxToggleIconButton`
- `MxToolBar`
- `MxStepper`
- `MxBreakpointProvider`
- `MxButtonGroup`
- `MxButtonFAB`
- `MxButtonFabMenu`
- `MxColorPicker`
- `MxField`
- `MxMessageBoxButton`
- `MxScrollToTop`
- `MxFocusTrap`
- `MxHighlighter`

### Olas de entrega
- ✅ OLA 1 completada (Core UI): Button, IconButton, TextField, Select, Checkbox, Radio, Switch, Snackbar, Dialog, Tooltip, Card, Table.
- ✅ OLA 2 completada (Data UX): DataGrid, DatePicker, DateRangePicker, TimePicker, Pagination, Skeleton, FileUpload, DropZone.
- ✅ OLA 3 completada (Navigation): AppBar, Drawer, NavMenu, Tabs, Breadcrumbs, Menu, TreeView.
- ✅ OLA 4 completada (Advanced): Carousel, Charts, Timeline, ChatBubble, Hotkey, SwipeArea, SplitPanel.
- ✅ OLA 5 completada (Utility & Feedback): Autocomplete, Avatar, Badge, Chip, List, Progress, Paper, Stack, Spacer, Divider, Overlay.
- ✅ OLA 6 completada (Forms & Content): NumericField, Slider, Rating, Container, Form, ExpansionPanel, Link, Image.
- ✅ OLA 7 completada (Interaction & Structure): ChipSet, Collapse, Grid, Hidden, Popover, ToggleGroup, ToggleIconButton, ToolBar, Stepper.
- ✅ OLA 8 iniciada (Behavior & Utilities): BreakpointProvider, ButtonGroup, ButtonFAB, ButtonFabMenu, ColorPicker, Field, MessageBox, ScrollToTop, FocusTrap, Highlighter.

> Nota: La lista completa solicitada se debe mapear contra MudBlazor y crear wrappers `Mx*` consistentes por API, estilos y accesibilidad.

## 2) Tokens semánticos

Se extendió el sistema con:
- Estados: success, warning, error, info.
- Interacciones: focus ring, hover overlay, active overlay, disabled.
- Elevación: sm, md, lg.
- Motion: fast, normal, slow.
- Z-index: base, dropdown, modal, tooltip.

## 3) Tipografía robusta

Se completó la escala tipográfica con:
- Headings `H1..H6`
- `Body1`, `Body2`, `Caption`, `Subtitle1`, `Subtitle2`, `Overline`, `Button`
- Soporte de `line-height` y `letter-spacing`

## 4) Documentación viva

Se recomienda publicar estas guías en un sitio interno (Storybook o equivalente):
- API de componentes.
- Do / Don't.
- Estados y variantes.
- Accesibilidad por componente.
- Changelog y migraciones.

## 5) Accesibilidad

Checklist mínimo:
- Contraste AA en texto y componentes de estado.
- Focus visible (ya agregado con `:focus-visible`).
- Navegación por teclado completa en componentes interactivos.
- ARIA labels en componentes sin texto visible.

## 6) Testing visual y de regresión

Pipeline recomendado:
1. Unit tests de tokens y utilidades.
2. Tests de interacción para wrappers `Mx*`.
3. Snapshot visual por componente/estado.
4. Gates automáticos en PR (build + test + accesibilidad + visual diff).
