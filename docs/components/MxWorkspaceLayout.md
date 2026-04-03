# MxWorkspaceLayout

## Propósito
`MxWorkspaceLayout` es el shell oficial de MachSoft para aplicaciones de trabajo. Separa explícitamente:
- `NavigationMenu` (navegación global),
- `LeftSidebar`/`RightSidebar` (paneles funcionales),
- `MainContent` (zona principal),
- `FooterContent` (opcional).

## Contrato público vigente

### Regiones declarativas
- `HeaderContent`
- `NavigationMenu`
- `LeftSidebar`
- `MainContent`
- `RightSidebar`
- `FooterContent`

### Parámetros
- `bool MainMenuOpen`
- `EventCallback<bool> MainMenuOpenChanged`
- `MxSidebarMode NavigationMenuMode`
- `string NavigationMenuWidth`
- `MxSidebarMode LeftSidebarMode`
- `MxSidebarMode RightSidebarMode`
- `EventCallback<MxSidebarMode> LeftSidebarModeChanged`
- `EventCallback<MxSidebarMode> RightSidebarModeChanged`
- `bool LeftSidebarOpen`
- `bool RightSidebarOpen`
- `EventCallback<bool> LeftSidebarOpenChanged`
- `EventCallback<bool> RightSidebarOpenChanged`
- `string LeftSidebarWidth`
- `string RightSidebarWidth`
- `EventCallback OnMenuToggle`
- `EventCallback OnBackdropClick`
- `string Class`
- `string Style`

> `NavigationMenuMode` permanece en API para evolución; la implementación estable actual de navegación principal es `Overlay`.

## Modelo funcional obligatorio de sidebars

`LeftSidebar` y `RightSidebar` tienen **dos modos mutuamente excluyentes**:

### 1) `Inline`
- Siempre visible.
- Ocupa espacio estructural real en el grid.
- Reduce el ancho de `MainContent`.

### 2) `Overlay`
- Visible solo cuando `*SidebarOpen = true`.
- Se renderiza superpuesto.
- No ocupa columna estructural.
- Usa backdrop.

No se aceptan estados ambiguos (por ejemplo “visible” sin ser inline ni overlay abierto).

## Header y control del shell
- Header fijo de `48px`.
- Incluye botón hamburguesa para `MainMenuOpen`.
- Incluye `MxMenu` de control del shell para:
  - cambiar `LeftSidebarMode` (`Inline` / `Overlay`),
  - abrir/cerrar `LeftSidebar` cuando está en `Overlay`,
  - cambiar `RightSidebarMode` (`Inline` / `Overlay`),
  - abrir/cerrar `RightSidebar` cuando está en `Overlay`.

Este menú controla estructura/estado del shell, **no sustituye** el contenido funcional de los sidebars.

## Reglas de layout del MainContainer
- `LeftSidebarMode = Inline` => descuenta `LeftSidebarWidth`.
- `RightSidebarMode = Inline` => descuenta `RightSidebarWidth`.
- Ambos inline => 3 columnas estructurales.
- Cualquier sidebar en overlay => no descuenta ancho.
- La columna central usa `minmax(0, 1fr)`.
- El desplazamiento visual de `MainContent` con overlays abiertos se resuelve con `transform` del contenedor principal (`--mx-main-visual-shift`), nunca agregando columnas inline.

## Separación técnica obligatoria (estructura vs overlay)
- Grid estructural: solo `Left inline` + `Main` + `Right inline`.
- Capa overlay separada (`mx-workspace-overlay-layer`) para:
  - `NavigationMenu` overlay
  - `LeftSidebar` overlay abierto
  - `RightSidebar` overlay abierto
- Backdrop independiente (`mx-workspace-backdrop`) activado cuando hay al menos un overlay visible.

## Estados de MainContainer
- `data-main-structural-span`: `full`, `center-with-left-inline`, `center-with-right-inline`, `center-with-both-inline`.
- `data-main-visual-state`: `neutral`, `shift-left`, `shift-right`, `shift-both`.
- Clases visuales asociadas:
  - `mx-workspace-main-region-neutral`
  - `mx-workspace-main-region-shift-left`
  - `mx-workspace-main-region-shift-right`
  - `mx-workspace-main-region-shift-both`

## Diferencia obligatoria: `NavigationMenu` vs sidebars funcionales
- `NavigationMenu`: navegación global de producto (overlay con `MainMenuOpen`).
- `LeftSidebar`/`RightSidebar`: paneles funcionales de la pantalla activa.

## Matriz resumida

| Configuración | Estructura | Overlay visible | Impacto en MainContent |
|---|---|---|---|
| Left inline, Right inline | Left + Main + Right | No | Pierde ancho por ambos |
| Left inline, Right overlay abierto | Left + Main | Right overlay | Pierde ancho solo por left |
| Left overlay abierto, Right inline | Main + Right | Left overlay | Pierde ancho solo por right |
| Left overlay cerrado, Right overlay cerrado | Main | No | No pierde ancho |

## Ejemplos actualizados

### Layout base con navegación
```razor
<MxWorkspaceLayout MainMenuOpen="@mainMenuOpen"
                   MainMenuOpenChanged="@(v => mainMenuOpen = v)">
  <NavigationMenu><MenuGlobal /></NavigationMenu>
  <MainContent><Dashboard /></MainContent>
</MxWorkspaceLayout>
```

### Left inline + Right overlay
```razor
<MxWorkspaceLayout LeftSidebarMode="MxSidebarMode.Inline"
                   RightSidebarMode="MxSidebarMode.Overlay"
                   RightSidebarOpen="@rightOpen"
                   RightSidebarOpenChanged="@(v => rightOpen = v)">
  <LeftSidebar><Filtros /></LeftSidebar>
  <MainContent><Listado /></MainContent>
  <RightSidebar><Detalle /></RightSidebar>
</MxWorkspaceLayout>
```

### Ambos overlay controlados por estado
```razor
<MxWorkspaceLayout LeftSidebarMode="MxSidebarMode.Overlay"
                   RightSidebarMode="MxSidebarMode.Overlay"
                   LeftSidebarOpen="@leftOpen"
                   LeftSidebarOpenChanged="@(v => leftOpen = v)"
                   RightSidebarOpen="@rightOpen"
                   RightSidebarOpenChanged="@(v => rightOpen = v)">
  <LeftSidebar><Comandos /></LeftSidebar>
  <MainContent><VistaTrabajo /></MainContent>
  <RightSidebar><Edicion /></RightSidebar>
</MxWorkspaceLayout>
```
