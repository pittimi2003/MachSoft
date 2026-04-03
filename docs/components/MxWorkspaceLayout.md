# MxWorkspaceLayout

## Propósito del componente
`MxWorkspaceLayout` es el shell reusable oficial de plataforma para vistas de trabajo. Su contrato separa de forma explícita:
- `NavigationMenu` (navegación global del producto),
- `LeftSidebar`/`RightSidebar` (paneles funcionales de la feature activa),
- `MainContent` (área principal adaptable),
- `FooterContent` (opcional).

## Cuándo usarlo
- Layout principal de apps y módulos con header persistente.
- Pantallas operativas que necesitan paneles laterales inline/overlay.
- Hosts de plataforma (`Showcase`, `Template.Server`, `Template.Wasm`) y nuevas apps basadas en `MachSoft.UI`.

## Cuándo no usarlo
- Vistas aisladas sin frame persistente.
- Flujos fullscreen (login, wizard de pantalla completa, landing).
- Componentes internos ya contenidos dentro de otro `MxWorkspaceLayout`.

---

## Contrato público soportado

### Regiones declarativas
- `HeaderContent`
- `NavigationMenu`
- `LeftSidebar`
- `MainContent`
- `RightSidebar`
- `FooterContent` (opcional)

### Parámetros públicos
- `bool MainMenuOpen`
- `EventCallback<bool> MainMenuOpenChanged`
- `MxSidebarMode NavigationMenuMode`
- `string NavigationMenuWidth`
- `bool LeftSidebarVisible`
- `bool RightSidebarVisible`
- `MxSidebarMode LeftSidebarMode`
- `MxSidebarMode RightSidebarMode`
- `string LeftSidebarWidth`
- `string RightSidebarWidth`
- `EventCallback OnMenuToggle`
- `EventCallback OnBackdropClick`
- `string Class`
- `string Style`

### Evento/estado de menú principal
- El botón hamburguesa vive en el header del shell.
- El botón alterna `MainMenuOpen`.
- `MainMenuOpen = false` por defecto.

### Enum público
`MxSidebarMode`:
- `Inline`: ocupa columna real y reduce ancho central.
- `Overlay`: se superpone y no cambia columnas centrales.

> **Nota de versión:** en esta iteración `NavigationMenuMode` se mantiene en API para evolución; la implementación estable de `NavigationMenu` es `Overlay`.

---

## Diferencia obligatoria: `NavigationMenu` vs `LeftSidebar`

### `NavigationMenu`
- Navegación global (módulos, secciones, acceso principal).
- Se controla con hamburguesa (`MainMenuOpen`).
- No forma parte del cálculo funcional left/right del workspace.

### `LeftSidebar`
- Panel funcional de la pantalla activa (CRUD, filtros, acciones contextuales).
- Puede ser `Inline` u `Overlay`.
- Sí participa del cálculo de ancho útil cuando está en `Inline`.

---

## Comportamiento funcional esperado

### Header
- Persistente y fijo.
- Altura contractual: `48px`.
- Franja mínima y limpia para branding/acciones globales.

### NavigationMenu
- Inicia cerrado (`MainMenuOpen = false`).
- Se abre/cierra desde hamburguesa.
- Se renderiza independiente de `LeftSidebar`.

### Sidebars funcionales (`LeftSidebar`, `RightSidebar`)
- Son opcionales por estado (`Visible`).
- `Inline`: reduce ancho útil de `MainContent`.
- `Overlay`: no reduce ancho útil de `MainContent`.

### MainContent
- Nunca usa ancho rígido.
- Se adapta por columnas inline activas.
- Permanece estable ante overlays.

### FooterContent
- Render condicional (solo si existe contenido).
- Ocupa 100% del ancho útil central.
- Ausente => sin huecos residuales.

### Backdrop
- Aparece cuando hay panel overlay visible (`NavigationMenu` o sidebars funcionales overlay).
- El shell delega reacción de negocio vía `OnBackdropClick`.

---

## Matriz de comportamiento obligatoria

### Navegación

| Caso | Visibilidad real | Impacto en MainContent | Backdrop | Interacción esperada |
|---|---|---|---|---|
| `MainMenuOpen = false` | `NavigationMenu` oculto | Ninguno | No | Workspace opera sin panel global abierto |
| `MainMenuOpen = true` | `NavigationMenu` visible (overlay) | Ninguno | Sí | Menú global superpuesto, cierre por hamburguesa o backdrop |

### Workspace funcional

| Left | Right | Visibilidad real de paneles | Impacto en ancho MainContent | Backdrop | Implicación visual/interacción |
|---|---|---|---|---|---|
| Cerrado | Cerrado | Ninguno | Ancho completo | No | Foco total en contenido |
| Inline | Cerrado | Left inline | Reduce por left | No | Panel operativo estructural |
| Cerrado | Inline | Right inline | Reduce por right | No | Panel contextual estructural |
| Inline | Inline | Ambos inline | Reduce por ambos | No | Tres columnas persistentes |
| Overlay | Cerrado | Left overlay | Sin reducción | Sí | Left flotante sobre contenido |
| Cerrado | Overlay | Right overlay | Sin reducción | Sí | Right flotante sobre contenido |
| Overlay | Overlay | Ambos overlay | Sin reducción | Sí | Ambos paneles superpuestos |
| Inline | Overlay | Left inline + right overlay | Reduce solo por left | Sí | Estructura izquierda + detalle flotante |
| Overlay | Inline | Left overlay + right inline | Reduce solo por right | Sí | Menor intrusión izquierda + estructura derecha |

---

## Qué controla el shell y qué no

### Controla el shell
- Estructura general.
- Header persistente y su hamburguesa.
- Render de `NavigationMenu`.
- Cálculo de columnas del workspace funcional.
- Comportamiento inline/overlay.
- Backdrop y transiciones.
- Ancho útil central.
- Footer opcional.

### No controla el shell
- Contenido de negocio del menú.
- Reglas CRUD de dominio.
- Formularios/entidades específicas.
- Acciones de negocio particulares de cada vista.

---

## Reglas de consumo para hosts

- `Showcase`, `Template.Server`, `Template.Wasm` deben consumir este contrato por slots y estado.
- Hosts no deben duplicar lógica de columnas, overlay, backdrop, header persistente ni menú hamburguesa.
- Hosts aportan solo estado + contenido de regiones + wiring mínimo.
- `NavigationMenu` no debe volver a modelarse como `LeftSidebar`.

---

## Ejemplos de consumo

### Layout mínimo
```razor
<MxWorkspaceLayout MainMenuOpen="@menuOpen" MainMenuOpenChanged="@(v => menuOpen = v)">
  <MainContent><VistaTrabajo /></MainContent>
</MxWorkspaceLayout>
```

### Con NavigationMenu
```razor
<MxWorkspaceLayout MainMenuOpen="@menuOpen" MainMenuOpenChanged="@(v => menuOpen = v)">
  <NavigationMenu><NavGlobal /></NavigationMenu>
  <MainContent><Dashboard /></MainContent>
</MxWorkspaceLayout>
```

### LeftSidebar inline
```razor
<MxWorkspaceLayout LeftSidebarVisible="true" LeftSidebarMode="MxSidebarMode.Inline" LeftSidebarWidth="300px">
  <LeftSidebar><PanelFiltros /></LeftSidebar>
  <MainContent><GridResultados /></MainContent>
</MxWorkspaceLayout>
```

### RightSidebar overlay
```razor
<MxWorkspaceLayout RightSidebarVisible="@detailOpen"
                   RightSidebarMode="MxSidebarMode.Overlay"
                   OnBackdropClick="@(() => detailOpen = false)">
  <MainContent><Listado /></MainContent>
  <RightSidebar><DetalleRegistro /></RightSidebar>
</MxWorkspaceLayout>
```

### Con footer
```razor
<MxWorkspaceLayout>
  <MainContent><Vista /></MainContent>
  <FooterContent><PaginacionEstado /></FooterContent>
</MxWorkspaceLayout>
```

### Ambos sidebars funcionales
```razor
<MxWorkspaceLayout LeftSidebarVisible="true"
                   RightSidebarVisible="@showEdit"
                   LeftSidebarMode="MxSidebarMode.Inline"
                   RightSidebarMode="MxSidebarMode.Overlay">
  <LeftSidebar><ComandosRapidos /></LeftSidebar>
  <MainContent><GridTrabajo /></MainContent>
  <RightSidebar><FormularioEdicion /></RightSidebar>
</MxWorkspaceLayout>
```

### Pantalla de trabajo real
```razor
<MxWorkspaceLayout MainMenuOpen="@menuOpen"
                   MainMenuOpenChanged="@(v => menuOpen = v)"
                   LeftSidebarVisible="true"
                   RightSidebarVisible="@detailOpen"
                   LeftSidebarMode="MxSidebarMode.Inline"
                   RightSidebarMode="MxSidebarMode.Overlay">
  <HeaderContent><ToolbarClientes /></HeaderContent>
  <NavigationMenu><MenuGlobal /></NavigationMenu>
  <LeftSidebar><FiltrosClientes /></LeftSidebar>
  <MainContent><GridClientes /></MainContent>
  <RightSidebar><FichaCliente /></RightSidebar>
  <FooterContent><EstadoOperacion /></FooterContent>
</MxWorkspaceLayout>
```

---

## Niveles de documentación
- **Contrato público estable:** regiones, parámetros y eventos listados arriba.
- **Comportamiento esperado estable:** separación `NavigationMenu`/`LeftSidebar`, reglas inline/overlay y adaptación de `MainContent`.
- **No garantizado como API:** clases CSS internas y detalle de composición interna.
