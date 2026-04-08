# MxWorkspaceLayout

## 1) Propósito del componente
`MxWorkspaceLayout` es el shell oficial de workspace para MachSoft. Su responsabilidad es separar explícitamente capas estructurales y capas flotantes del shell. También reemplaza **conceptualmente** el rol que en el benchmark de origen cumplía `MlxMainContainer`, pero sin replicar su componente ni su identidad visual:
- `Header`
- `NavigationMenu`
- `LeftSidebar`
- `MainContainer` (`MainContent`)
- `RightSidebar`
- `Footer`

## 2) Cuándo usarlo
Úsalo cuando una pantalla/app necesite:
- navegación global (`NavigationMenu`) independiente de paneles funcionales,
- sidebars funcionales con modo `Inline`/`Overlay`,
- superficie principal de trabajo con reglas estructurales consistentes,
- header persistente fijo de 48px.

## 3) Cuándo no usarlo
No usarlo para:
- páginas simples sin shell de workspace,
- layouts temporales/prototipo,
- composiciones que mezclen navegación global como `LeftSidebar`.

## 4) Contrato público

### Regiones declarativas mínimas
- `HeaderContent`
- `NavigationMenu`
- `LeftSidebar`
- `MainContent`
- `RightSidebar`
- `FooterContent`

### Parámetros públicos
- `bool MainMenuOpen`
- `EventCallback<bool> MainMenuOpenChanged`
- `MxSidebarMode NavigationMenuMode`
- `string NavigationMenuWidth`
- `MxSidebarMode LeftSidebarMode`
- `bool LeftSidebarOpen`
- `EventCallback<bool> LeftSidebarOpenChanged`
- `string LeftSidebarWidth`
- `MxSidebarMode RightSidebarMode`
- `bool RightSidebarOpen`
- `EventCallback<bool> RightSidebarOpenChanged`
- `string RightSidebarWidth`

Parámetros complementarios del shell:
- `EventCallback<MxSidebarMode> LeftSidebarModeChanged`
- `EventCallback<MxSidebarMode> RightSidebarModeChanged`
- `EventCallback OnMenuToggle`
- `EventCallback OnBackdropClick`
- `string? Class`
- `string? Style`

> `NavigationMenuMode` se mantiene por compatibilidad/evolución, pero en la iteración actual el comportamiento efectivo del menú principal es `Overlay`.

### Parámetros de ancho de sidebars (contrato obligatorio)
- `LeftSidebarWidth`
  - valor por defecto: `"320px"` en el contrato `MxWorkspaceLayout`.
  - controla el ancho real del `LeftSidebar` cuando está visible (`Inline` u `Overlay`).
  - participa en el cálculo estructural del `MainContainer` cuando `LeftSidebarMode = Inline`.
- `RightSidebarWidth`
  - valor por defecto: `"360px"` en el contrato `MxWorkspaceLayout`.
  - controla el ancho real del `RightSidebar` cuando está visible (`Inline` u `Overlay`).
  - participa en el cálculo estructural del `MainContainer` cuando `RightSidebarMode = Inline`.

Regla de implementación: no definir anchos mágicos de sidebars fuera del contrato; los estilos consumen variables CSS inyectadas por parámetros (`--mx-left-sidebar-width`, `--mx-right-sidebar-width`).


## 5) Diferencia oficial: `NavigationMenu` vs sidebars funcionales
- `NavigationMenu`: navegación global de aplicación, capa overlay independiente del grid del workspace.
- `LeftSidebar`/`RightSidebar`: paneles funcionales de la vista activa (filtros, detalle, edición contextual).

Regla: **`NavigationMenu` no puede modelarse como `LeftSidebar`**.

Semántica oficial obligatoria:
- `NavigationMenu` = navegación global de la aplicación.
- `LeftSidebar` = región izquierda funcional de la vista activa.
- `MainContent` = superficie principal de trabajo.
- `RightSidebar` = región contextual derecha de la vista activa.

## 6) Definición oficial de `Inline`
`Inline` significa:
- siempre visible,
- estructural,
- ocupa columna real del grid,
- reduce el ancho del `MainContainer`,
- no usa backdrop,
- no depende de `Open`.

## 7) Definición oficial de `Overlay`
`Overlay` significa:
- flotante/superpuesto,
- abierto/cerrado por estado `Open`,
- no estructural,
- usa backdrop cuando está abierto,
- cerrado no reserva ancho.

## 8) Reglas oficiales del `MainContainer`
- `MainContainer` **solo cede espacio estructural** a sidebars en `Inline`.
- El grid estructural usa `left inline | minmax(0, 1fr) | right inline`.
- Sidebars `Overlay` no forman parte del cálculo de columnas.
- Overlays abiertos no desplazan el `MainContainer`; se renderizan por encima como capa modal sin alterar la posición del contenido principal.
- Overlays cerrados no reservan ancho en la columna central.
- En responsive (`max-width: 1024px`), cuando `RightSidebar` está en `Overlay` y abierto con `EnableMobileContextualDetailMode=true`, el panel derecho reemplaza visualmente al `MainContent` (patrón browse/detail/edit en móvil) e incluye acción explícita de cierre contextual.

## 9) Ejemplos de uso

### Shell base con navegación global
```razor
<MxWorkspaceLayout MainMenuOpen="@menuOpen"
                   MainMenuOpenChanged="@(v => menuOpen = v)">
    <NavigationMenu><AppNav /></NavigationMenu>
    <MainContent><Dashboard /></MainContent>
</MxWorkspaceLayout>
```

### Left inline + Right overlay
```razor
<MxWorkspaceLayout LeftSidebarMode="MxSidebarMode.Inline"
                   RightSidebarMode="MxSidebarMode.Overlay"
                   RightSidebarOpen="@rightOpen"
                   RightSidebarOpenChanged="@(v => rightOpen = v)">
    <LeftSidebar><Filters /></LeftSidebar>
    <MainContent><ListPage /></MainContent>
    <RightSidebar><Details /></RightSidebar>
</MxWorkspaceLayout>
```

### Ambos overlay
```razor
<MxWorkspaceLayout LeftSidebarMode="MxSidebarMode.Overlay"
                   RightSidebarMode="MxSidebarMode.Overlay"
                   LeftSidebarOpen="@leftOpen"
                   LeftSidebarOpenChanged="@(v => leftOpen = v)"
                   RightSidebarOpen="@rightOpen"
                   RightSidebarOpenChanged="@(v => rightOpen = v)">
    <LeftSidebar><Commands /></LeftSidebar>
    <MainContent><Workbench /></MainContent>
    <RightSidebar><Inspector /></RightSidebar>
</MxWorkspaceLayout>
```

## 10) Matriz de comportamiento oficial

| Caso | Qué se ve | Qué ocupa espacio estructural | Backdrop | Comportamiento de `MainContainer` |
|---|---|---|---|---|
| NavigationMenu cerrado | Header + workspace normal | Solo sidebars inline | No | Ancho definido solo por inline |
| NavigationMenu abierto | NavigationMenu overlay + workspace | Solo sidebars inline | Sí | Sin cambio estructural por navegación |
| Left inline / Right cerrado (overlay) | Left visible + main | Left | No (si ningún overlay abierto) | Cede ancho solo a left |
| Left cerrado (overlay) / Right inline | Right visible + main | Right | No (si ningún overlay abierto) | Cede ancho solo a right |
| Left inline / Right inline | Left + main + right visibles | Left + Right | No | Cede ancho a ambos |
| Left overlay cerrado / Right cerrado | Solo main | Ninguno | No | Ocupa ancho completo |
| Left cerrado / Right overlay cerrado | Solo main | Ninguno | No | Ocupa ancho completo |
| Left overlay abierto / Right cerrado | Left overlay + main | Ninguno | Sí | Estructuralmente full-width; main fijo (sin desplazamiento) |
| Left cerrado / Right overlay abierto | Right overlay + main | Ninguno | Sí | Estructuralmente full-width; main fijo (sin desplazamiento) |
| Left overlay abierto / Right overlay abierto | Ambos overlays + main | Ninguno | Sí | Estructuralmente full-width; main fijo (sin desplazamiento) |
| Left inline / Right overlay | Left inline + right overlay (si abierto) | Left | Sí si right overlay abierto | Cede ancho solo a left |
| Left overlay / Right inline | Right inline + left overlay (si abierto) | Right | Sí si left overlay abierto | Cede ancho solo a right |

## Reglas de implementación interna
- Header fijo: `48px`.
- `NavigationMenu` overlay separado del grid.
- Grid estructural exclusivo para regiones inline.
- Capa overlay separada para `NavigationMenu`, `LeftSidebar` overlay, `RightSidebar` overlay.
- Backdrop independiente para overlays visibles.
