# MxWorkspaceLayout

## Propósito del componente
`MxWorkspaceLayout` es el shell oficial reutilizable de plataforma para vistas tipo workspace en `MachSoft.UI`. Expone un contrato declarativo de regiones y estado para mantener header persistente, sidebars (inline/overlay), contenedor central adaptable y footer opcional, sin acoplarse a casos de negocio.

## Cuándo usarlo
- Pantallas de trabajo con navegación persistente + contenido central dinámico.
- Flujos CRUD con panel lateral de operaciones/contexto.
- Pantallas que requieran panel derecho para detalle/edición contextual.
- Hosts de plataforma (`Showcase`, `Template.Server`, `Template.Wasm`) y nuevas apps que consumen `MachSoft.UI`.

## Cuándo no usarlo
- Vistas aisladas sin shell global persistente.
- Componentes internos embebidos dentro de otra región ya gobernada por `MxWorkspaceLayout`.
- Páginas que requieren estructura totalmente distinta (wizard fullscreen, landing, login sin frame).

## Contrato público soportado

### Regiones declarativas
- `HeaderContent`
- `LeftSidebar`
- `MainContent`
- `RightSidebar`
- `FooterContent` (opcional)

### Parámetros públicos
- `bool LeftSidebarVisible`
- `bool RightSidebarVisible`
- `MxSidebarMode LeftSidebarMode`
- `MxSidebarMode RightSidebarMode`
- `string LeftSidebarWidth`
- `string RightSidebarWidth`
- `bool MainMenuOpen`

### Eventos públicos
- `EventCallback<bool> MainMenuOpenChanged`
- `EventCallback OnMenuToggle`
- `EventCallback OnBackdropClick`

### Enum público
- `MxSidebarMode`
  - `Inline`: ocupa columna real y reduce ancho central.
  - `Overlay`: se superpone y no altera columnas centrales.

## Comportamiento funcional del shell

### Header
- Es persistente y fijo.
- Altura contractual: `48px`.
- Incluye botón hamburguesa integrado en el shell.
- El estado `MainMenuOpen` inicia en `false` por defecto.

### Sidebars
- Solo aparecen cuando su visibilidad está activada (`LeftSidebarVisible` / `RightSidebarVisible`).
- Se renderizan en `Inline` u `Overlay` según su modo configurado.
- La lógica de cálculo de columnas y overlays está encapsulada dentro del shell.

### MainContainer
- Ocupa todo el ancho útil cuando ambos sidebars están cerrados.
- Reduce ancho solo por sidebars `Inline`.
- No pierde ancho por sidebars `Overlay`.

### Footer
- Es opcional.
- Si existe, usa el 100% del ancho útil central.
- Si no existe, no deja huecos residuales.

### Backdrop
- Se muestra cuando hay al menos un sidebar en `Overlay` visible.
- La reacción de negocio al click de backdrop se delega mediante `OnBackdropClick`.

## Matriz de comportamiento obligatoria

| Left | Right | Paneles visibles | Impacto en ancho MainContainer | Backdrop | Comportamiento esperado |
|---|---|---|---|---|---|
| Cerrado | Cerrado | Ninguno | Ancho completo | No | Flujo foco total en contenido central |
| Inline | Cerrado | Left inline | Reduce por ancho left | No | Navegación/panel operativo estable sin superposición |
| Cerrado | Inline | Right inline | Reduce por ancho right | No | Panel contextual fijo coexistiendo con contenido |
| Inline | Inline | Ambos inline | Reduce por ambos anchos | No | Workspace de tres columnas persistentes |
| Overlay | Cerrado | Left overlay | Sin reducción | Sí | Left se superpone; contenido mantiene ancho |
| Cerrado | Overlay | Right overlay | Sin reducción | Sí | Right contextual modal lateral; contenido estable |
| Overlay | Overlay | Ambos overlay | Sin reducción | Sí | Ambos paneles flotan sobre contenido central |
| Inline | Overlay | Left inline + right overlay | Reduce solo por left | Sí | Operación izquierda fija + detalle derecho contextual |
| Overlay | Inline | Left overlay + right inline | Reduce solo por right | Sí | Contexto izquierdo flotante + detalle derecho estructural |

## Responsabilidades del shell vs contenido

### Controla el shell
- Estructura general y jerarquía de regiones.
- Persistencia del header fijo de 48px.
- Cálculo de columnas para sidebars inline.
- Render y coordinación de sidebars overlay.
- Backdrop y transición visual del frame.
- Ancho útil del contenedor central.
- Presencia opcional del footer central.
- Interacción base del menú principal (hamburguesa + `MainMenuOpen`).

### No controla el shell
- Navegación de negocio específica.
- Reglas CRUD de dominio.
- Formularios o entidades concretas.
- Reglas funcionales particulares de cada pantalla.
- Decisiones de flujo propias del host más allá de estado/slots.

## Reglas de consumo

### Showcase
- Consumir `MxWorkspaceLayout` como host principal.
- Demostrar estados inline/overlay únicamente mediante parámetros públicos.
- No inyectar cálculos internos de layout en el host.

### Template.Server y Template.Wasm
- Consumir `MxWorkspaceLayout` en `MainLayout`.
- Usar sidebars/regiones por slots declarativos.
- No replicar estructura de grid, overlay o backdrop fuera del componente.

### Nueva vista de negocio
- Definir contenido de regiones con `RenderFragment`.
- Manejar solo estado de visibilidad/modo/ancho, sin reimplementar shell.
- Mantener desacople entre contenido de negocio y frame de layout.

### Qué no deben hacer los hosts
- No construir layouts paralelos con `MudLayout/MudDrawer` para resolver este shell.
- No duplicar lógica de columnas, overlays o backdrop.
- No acoplar `MainContent` a un caso de negocio único.

## Ejemplos de consumo

### 1) Layout mínimo
```razor
<MxWorkspaceLayout MainMenuOpen="@menuOpen" MainMenuOpenChanged="@(v => menuOpen = v)">
  <MainContent>
    <MiVista />
  </MainContent>
</MxWorkspaceLayout>
```

### 2) Left sidebar inline
```razor
<MxWorkspaceLayout LeftSidebarVisible="true"
                   LeftSidebarMode="MxSidebarMode.Inline"
                   LeftSidebarWidth="280px">
  <LeftSidebar><MiPanelOperativo /></LeftSidebar>
  <MainContent><MiGrid /></MainContent>
</MxWorkspaceLayout>
```

### 3) Right sidebar overlay
```razor
<MxWorkspaceLayout RightSidebarVisible="@showDetail"
                   RightSidebarMode="MxSidebarMode.Overlay"
                   RightSidebarWidth="360px"
                   OnBackdropClick="@(() => showDetail = false)">
  <MainContent><MiListado /></MainContent>
  <RightSidebar><DetalleSeleccionado /></RightSidebar>
</MxWorkspaceLayout>
```

### 4) Layout con footer
```razor
<MxWorkspaceLayout>
  <MainContent><MiDashboard /></MainContent>
  <FooterContent><MiPaginacion /></FooterContent>
</MxWorkspaceLayout>
```

### 5) Ambos paneles
```razor
<MxWorkspaceLayout LeftSidebarVisible="true"
                   RightSidebarVisible="@showEdit"
                   LeftSidebarMode="MxSidebarMode.Inline"
                   RightSidebarMode="MxSidebarMode.Overlay">
  <LeftSidebar><FiltrosRapidos /></LeftSidebar>
  <MainContent><GridClientes /></MainContent>
  <RightSidebar><FormularioEdicion /></RightSidebar>
</MxWorkspaceLayout>
```

### 6) Pantalla de trabajo real
```razor
<MxWorkspaceLayout LeftSidebarVisible="@menuOpen"
                   RightSidebarVisible="@detailOpen"
                   LeftSidebarMode="MxSidebarMode.Overlay"
                   RightSidebarMode="MxSidebarMode.Inline"
                   MainMenuOpen="@menuOpen"
                   MainMenuOpenChanged="@(v => menuOpen = v)">
  <HeaderContent><ToolbarClientes /></HeaderContent>
  <LeftSidebar><NavClientes /></LeftSidebar>
  <MainContent><GridClientes /></MainContent>
  <RightSidebar><FichaCliente /></RightSidebar>
  <FooterContent><StatusOperacion /></FooterContent>
</MxWorkspaceLayout>
```

## Notas de estabilidad del contrato
- **Contrato público estable:** regiones, parámetros y eventos documentados en este archivo.
- **Comportamiento esperado estable:** semántica inline/overlay y reglas de ancho central.
- **Detalle interno no garantizado:** estructura CSS interna exacta, clases privadas y estrategia de transición.
