# CATALOG_EXECUTION_STATUS — Estado operativo del catálogo MachSoft

> Archivo vivo. Fuente de verdad para continuidad operativa del catálogo.

## 1) Estado actual del sistema
- **Estado general:** En formalización operativa con gobernanza documental activa.
- **Cobertura documental:** Contrato permanente (`/AGENTS.md`) y estado operativo (`/docs/CATALOG_EXECUTION_STATUS.md`) establecidos.
- **Uso esperado:** referencia obligatoria para retomar contexto y validar cierres en futuras ejecuciones.

## 2) Decisiones cerradas
- Se adopta `AGENTS.md` raíz como contrato permanente de trabajo de Codex para el catálogo.
- Se adopta `docs/CATALOG_EXECUTION_STATUS.md` como archivo vivo de estado operativo.
- Se establece obligación de actualizar este archivo en cada ejecución relevante que cambie el estado real.

## 3) Decisiones que no deben reabrirse
- No aceptar mínimos como cierre.
- No aceptar wrappers triviales como solución.
- No aceptar demo/prototipo como cierre operativo.
- No exponer `Mud*` en API pública de negocio; usar `Mx*`.
- No tratar MudBlazor como lenguaje de API pública; solo implementación interna.
- No saltar validación de build real.
- No cerrar con errores Razor.
- No editar archivos generados.

## 4) Controles aceptados
- **Control de contrato técnico:** reglas permanentes versionadas en `AGENTS.md` raíz.
- **Control de continuidad:** estado operativo centralizado en este documento.
- **Control de trazabilidad:** decisiones críticas y objetivo inmediato documentados en secciones explícitas.

## 5) Controles no cerrados
- Definir check-list automatizable de cumplimiento (build, Razor, actualización de estado, evidencia de showcase).
- Definir convención de severidad para desviaciones (bloqueante, mayor, menor).
- Definir ciclo formal de revisión/aprobación de cambios sobre controles.

## 6) Controles que deben rehacerse o elevarse
- Elevar control de “showcase auditable” con criterio objetivo mínimo de evidencia (ruta, componente, versión).
- Rehacer control de “documentación obligatoria” con plantilla de actualización para evitar ambigüedad.
- Elevar control de contratos `Mx*` con validación estática o revisión sistemática en PR.

## 7) Decisiones críticas vigentes
- La autoridad visual permanece en MachSoft Design System.
- La superficie pública de componentes de negocio debe mantenerse en `Mx*`.
- La validez de cierre depende de build real sin errores, incluyendo Razor.
- La continuidad operativa depende de mantener este archivo actualizado por cada cambio relevante.

## 8) Objetivo inmediato
- **Objetivo vigente:** mantener este estado operativo como fuente de verdad activa y exigir su actualización en cada ejecución relevante del catálogo.

## 9) Último cierre válido / última actualización relevante
- **Fecha:** 2026-04-07 (UTC).
- **Tipo:** realineación de composición `/work` contra guía estructural Figma + Design System oficial.
- **Resultado:** `/work` actualizado en Demo/Template (Server + WebAssembly) con layout estructural alineado (cabecera operativa, rail, grid dominante y panel contextual) usando contratos `Mx*`.
- **Impacto operativo:** vista `/work` queda gobernada por composición reusable del sistema (sin estilos hardcoded de Figma ni exposición pública de `Mud*`).

---

## Regla operativa obligatoria (aplicación continua)
Toda ejecución relevante que cambie estado real del catálogo **debe** incluir actualización de este archivo en el mismo conjunto de cambios.


## 10) Actualización 2026-04-03 (UTC) — Shell oficial de workspace
- Se incorpora `MxWorkspaceLayout` como contrato público reusable en `MachSoft.UI`.
- Se incorpora `MxSidebarMode` para gobernar sidebars inline/overlay sin exponer `Mud*` en API pública.
- `MachSoft.UI.Showcase`, `MachSoft.Template.Server` y `MachSoft.Template.Wasm` migran su layout principal para consumir el nuevo shell `Mx*`.
- Se agrega documentación contractual en `docs/components/MxWorkspaceLayout.md` con matriz obligatoria de comportamiento y reglas de consumo.
- Se agregan pruebas de componente para estados cerrados/inline/overlay, footer opcional, toggle de menú y casos sin contenido de región.


## 11) Actualización 2026-04-03 (UTC) — Adopción real del shell en hosts
- Se verifica y consolida que `MainLayout` en `MachSoft.UI.Showcase`, `MachSoft.Template.Wasm` y `MachSoft.Template.Server` consume `MxWorkspaceLayout` como layout real (no como demo incrustada).
- Se elimina wiring decorativo en Showcase (`OnMenuToggle` sin efecto), manteniendo solo estado + slots + navegación/controles mínimos del host.
- Se agrega cobertura de pruebas para vigilar que los hosts no reintroduzcan `MudLayout`, `MudDrawer`, `MudMainContent` o `MudAppBar` en `MainLayout`.
- Resultado operativo: el shell `MxWorkspaceLayout` queda como única pieza estructural del layout en los tres hosts consumidores.

## 12) Actualización 2026-04-03 (UTC) — Corrección arquitectónica de `NavigationMenu` vs `LeftSidebar`
- Se corrige el contrato de `MxWorkspaceLayout` para separar explícitamente `NavigationMenu` (navegación global) de `LeftSidebar` (panel funcional).
- Se mantiene header fijo de 48px con botón hamburguesa embebido en el shell y `MainMenuOpen` cerrado por defecto.
- `NavigationMenuMode` queda expuesto en API para evolución futura, con soporte estable actual en `Overlay` sin acoplarse al cálculo funcional left/right.
- `MachSoft.UI.Showcase`, `MachSoft.Template.Wasm` y `MachSoft.Template.Server` migran a `NavigationMenu` y dejan de modelar el menú principal como `LeftSidebar`.
- Se refuerzan pruebas bUnit del layout para independencia de navegación, composición inline/overlay, footer opcional y comportamiento de backdrop.
- Se actualiza documentación contractual (`docs/components/MxWorkspaceLayout.md`) con matriz de comportamiento de navegación + workspace y reglas de consumo por host.

## 13) Actualización 2026-04-03 (UTC) — Modelo funcional definitivo de sidebars en `MxWorkspaceLayout`
- Se elimina el modelo ambiguo `Visible + Mode` para `LeftSidebar`/`RightSidebar` y se formaliza contrato con `LeftSidebarMode`/`RightSidebarMode` + `LeftSidebarOpen`/`RightSidebarOpen`.
- Regla contractual implementada: `Inline` siempre visible y estructural; `Overlay` solo visible cuando `*SidebarOpen = true` y siempre con backdrop.
- `MainContent` pasa a calcular columnas exclusivamente con sidebars `Inline`, manteniendo `minmax(0, 1fr)` en la columna central y sin contaminación estructural por overlays.
- Header de 48px mantiene hamburguesa de menú principal e incorpora `MxMenu` de control de shell para cambio de modo/apertura de sidebars funcionales.
- `MachSoft.UI.Showcase`, `MachSoft.Template.Wasm` y `MachSoft.Template.Server` consumen el contrato actualizado de forma declarativa.
- Se actualiza documentación contractual y se amplía bUnit para cubrir combinaciones left/right inline/overlay, overlays cerrados/abiertos, independencia del menú principal y menú de control del shell.

## 14) Actualización 2026-04-03 (UTC) — Corrección funcional de overlay y separación estructural/visual en `MxWorkspaceLayout`
- Se refuerza la arquitectura del shell para separar explícitamente: grid estructural (solo inline), capa de overlays (`mx-workspace-overlay-layer`) y backdrop independiente.
- Se mantiene el cálculo estructural del `MainContainer` exclusivamente con sidebars en modo `Inline`; sidebars `Overlay` cerrados no reservan ancho en ninguna columna.
- Se agrega estado visual del contenedor principal (`data-main-visual-state` + clases `mx-workspace-main-region-*`) y desplazamiento por `transform` usando `--mx-main-visual-shift` para acompañar overlays abiertos sin convertirlos en columnas inline.
- Se actualiza Showcase para arrancar en modo overlay cerrado y exponer en footer el estado estructural + estado de desplazamiento visual, facilitando auditoría explícita de comportamiento real.
- Se amplía cobertura bUnit para validar full-width estructural cuando overlays están cerrados, visibilidad de paneles/backdrop al abrir overlays, estado visual del main y reducción estructural correcta en modos inline.

## 15) Actualización 2026-04-03 (UTC) — Consolidación semántica de `MxWorkspaceLayout` (Mode/Open + separación estructural)
- Se consolida el contrato semántico del shell: `Inline` = siempre visible y estructural; `Overlay` = abierto/cerrado y no estructural.
- Se mantiene `NavigationMenu` como capa independiente del workspace grid, con comportamiento efectivo `Overlay` y apertura controlada por hamburguesa (`MainMenuOpen`).
- El cálculo de columnas del `MainContainer` queda restringido a sidebars en `Inline`; overlays cerrados no reservan ancho estructural.
- Se preserva el desplazamiento visual opcional del contenido principal ante overlays abiertos mediante `transform`, separado explícitamente del cálculo estructural del grid.
- Se amplía cobertura bUnit para footer opcional y para garantizar que al cambiar de `Overlay` a `Inline` desde el menú shell se cierre estado `Open` residual.
- Templates Server/Wasm adoptan callbacks de cambio de modo para preservar consistencia del control de shell desde header.

## 16) Actualización 2026-04-03 (UTC) — Estado inicial oficial del shell + composición central auditable en Showcase
- Se corrige el estado inicial de hosts para que `LeftSidebar` y `RightSidebar` arranquen en `Overlay` y cerrados en Showcase, Template.Wasm y Template.Server.
- Se mantiene `NavigationMenu` en modo efectivo `Overlay` con `MainMenuOpen` cerrado por defecto, asegurando arranque limpio sin dominancia visual lateral.
- Se refuerza la página inicial del Showcase con composición central fluida (región principal + paneles secundarios + grid operativo responsive) para evidenciar visualmente la ocupación plena del `MainContainer`.
- Se agrega cobertura de pruebas en `WorkspaceHostAdoptionTests` para vigilar que los tres hosts mantengan sidebars en `Overlay` y cerrados por defecto.
- Resultado operativo: con overlays cerrados no hay reserva estructural lateral; al abrir overlays el comportamiento sigue siendo visual (overlay/transform) y no de columnas inline.

## 17) Actualización 2026-04-03 (UTC) — Composición inicial premium del `MainContainer` en Showcase
- Se extiende `MxPageContainer` con `FullWidth` para habilitar páginas de evidencia visual que deben ocupar el ancho útil completo del workspace.
- La Home del Showcase adopta `MxPageContainer FullWidth` y una composición interna fluida con hero, matriz central y tablero operativo multibloque para evitar percepción de columna angosta.
- Se formaliza estilo responsive de `mx-home-*` en `MachSoft.UI` para que el contenido central use ancho real disponible sin introducir sidebars inline por defecto.
- Se mantiene el estado inicial contractual del shell (`NavigationMenu` cerrado, `LeftSidebar/RightSidebar` en `Overlay` y cerrados), de forma que la dominancia visual recaiga en `MainContainer`.

## 18) Actualización 2026-04-03 (UTC) — Redistribución oficial de regiones en Showcase (`LeftSidebar`/`MainContainer`/`RightSidebar`)
- Se refactoriza `MainLayout` de `MachSoft.UI.Showcase` para impedir que `LeftSidebar` y `RightSidebar` actúen como contenedores de contenido principal.
- `LeftSidebar` queda delimitado como panel operativo lateral compacto (acciones rápidas, filtros y comandos operativos), alineado con la regla contractual de región secundaria.
- `MainContainer` conserva y exhibe la composición principal real de la pantalla (hero, cards informativas, dashboard y grid central) mediante `@Body` como slot dominante.
- `RightSidebar` queda reservado para contexto profundo (detalle, inspección y acciones de edición/creación contextual), sin trasladar dominancia visual fuera del contenido central.
- Se ajusta `LeftSidebarWidth` a un ancho lateral razonable (272px) para escenarios `Inline`, evitando lectura de contenedor principal lateral.
- Se mantiene el arranque contractual: `NavigationMenu` cerrado + `LeftSidebar`/`RightSidebar` en `Overlay` cerrados por defecto, con `MainContainer` visible y dominante desde carga inicial.

## 19) Actualización 2026-04-03 (UTC) — Plantilla central reusable para páginas del Showcase
- Se introduce `ShowcasePageLayout` en `MachSoft.UI.Showcase` como plantilla visual común para páginas del catálogo renderizadas en `MainContent`.
- La plantilla estandariza encabezado, subtítulo, resumen opcional, bloque principal dominante y rail contextual opcional sin alterar la separación estructural del `MainLayout`.
- Se migra el set principal de rutas del Showcase (home, foundations, components, patterns, states y audit) para consumir la plantilla común y eliminar composición de columna angosta heredada.
- Se incorporan estilos `mx-showcase-page-*` en `machsoft-ui.css` para ocupar ancho central real, mantener cards coherentes y usar grid/stack fluido con degradación responsive.
- Resultado operativo: el contenido de `@Body` se percibe como región dominante del workspace, manteniendo `NavigationMenu`, `LeftSidebar` y `RightSidebar` como regiones independientes del shell.

## 20) Actualización 2026-04-04 (UTC) — Composición central reusable reforzada para páginas del Showcase
- Se refuerza `ShowcasePageLayout` como base reusable de composición en `MainContent`, incorporando frame centrado, región de encabezado opcional (`HeaderContent`), resumen opcional, región principal fluida y rail contextual opcional.
- Se ajusta la distribución visual para priorizar explícitamente la columna central (`mx-showcase-page-main` con proporción dominante) y evitar percepción de columna angosta anclada a la izquierda.
- Se migra la Home (`/`) y Foundations Colors (`/foundations/colors`) a la base reusable reforzada, unificando patrones visuales antes implementados como caso especial aislado.
- Se migran además páginas equivalentes de foundations (`/foundations/spacing`, `/foundations/typography`) para reducir divergencia de composición entre rutas principales del catálogo.
- Se amplían estilos de `machsoft-ui.css` con clases `mx-showcase-*` para hero, grids dominantes, bloques de intención, token grids y rail contextual, manteniendo sidebars del shell como regiones secundarias.
- Resultado operativo: el contenido renderizado en `@Body` recupera dominancia visual consistente dentro de `MainContent` sin tocar contrato estructural de `MxWorkspaceLayout`.

## 21) Actualización 2026-04-06 (UTC) — Eliminación definitiva de `ShowcasePageLayout`
- Se elimina `ShowcasePageLayout.razor` de `MachSoft.UI.Showcase` para evitar doble layout estructural dentro de `MainContent`.
- Todas las páginas del Showcase que dependían de `ShowcasePageLayout` migran a composición directa en `@Body` usando `MxPageContainer`, `MxSectionCard`, grids/stacks locales y componentes propios de cada vista.
- La Home (`/`) y Foundations (`/foundations/colors`, `/foundations/spacing`, `/foundations/typography`) reemplazan regiones implícitas (`HeaderContent`, `ChildContent`, `ContextContent`) por composición explícita y local sin wrapper estructural adicional.
- Se preserva la jerarquía del shell: `MainLayout` + `MxWorkspaceLayout` como único layout global; `LeftSidebar`/`RightSidebar` continúan como regiones secundarias.
- Resultado operativo: la región dominante vuelve a ser el contenido central real renderizado en `MainContent`, sin layouts de página anidados.

## 22) Actualización 2026-04-06 (UTC) — Corrección de composición CSS en Home dentro de `MainContent`
- Se corrigen reglas de `MxPageContainer` para formalizar que `FullWidth=true` use ancho útil real: `width:100%`, `min-width:0`, `box-sizing:border-box`, `max-width:none`, `margin:0` y `padding-inline` fluido en modo full.
- Se elimina la restricción visual de composición angosta en Home ajustando `mx-home-shell-evidence` y los grids `mx-showcase-*` para usar columnas fluidas sin mínimos rígidos que comprimían la región principal.
- Se amplía `mx-showcase-metrics` a distribución horizontal (3 columnas) en desktop y degradación a una sola columna en breakpoint responsive.
- Se refuerza robustez de ancho con `min-width:0` en regiones críticas (`mx-showcase-main-primary`, `mx-showcase-main-side`, `mx-showcase-main-operational`, `mx-showcase-main-rail`, `mx-showcase-main-surface-grid`) para evitar colapso visual por contenido.
- Resultado operativo: la Home deja de percibirse como columna estrecha pegada a la izquierda y pasa a ocupar el `MainContent` completo con jerarquía central dominante.

## 23) Actualización 2026-04-06 (UTC) — Auditoría y depuración de duplicados CSS de Home
- Se audita `machsoft-ui.css` y se elimina el bloque legado duplicado de clases `mx-home-*` que redefinía composición de Home en una segunda zona del archivo.
- Se conserva una única fuente de verdad para Home (`mx-home-main-grid`, `mx-home-main-surface-grid`, `mx-home-hero`, `mx-home-kicker`, `mx-home-hero-tags`, `mx-home-hero-metrics`, `mx-home-rules`, `mx-home-ops-kpis` y breakpoint asociado).
- Se evita conflicto de cascada donde reglas antiguas (declaradas antes) imponían `box-shadow`, fondos y paddings distintos respecto al bloque vigente, generando resultados visuales inconsistentes según orden y combinación de clases.
- Se preservan estilos no relacionados con Home (`mx-forms-grid`, `mx-token-grid`, `mx-foundation-*`, `mx-table`) para mantener estabilidad del resto del showcase.
- Resultado operativo: Home queda con contrato visual único y predecible dentro de `MainContent`, sin doble definición competidora.

## 24) Actualización 2026-04-06 (UTC) — Verificación de alineación runtime vs fuente (sin refactor adicional)
- Se ejecuta limpieza operativa completa de runtime local: cierre de procesos `dotnet run`/`dotnet watch` del host, limpieza de `bin/obj`, `restore` y `build` limpio de la solución.
- Se levanta únicamente `MachSoft.UI.Showcase` desde cero y se verifica que la hoja cargada en runtime es `/_content/MachSoft.UI/machsoft-ui.css`.
- Se valida coincidencia exacta entre CSS servido y fuente (`src/MachSoft.UI/wwwroot/machsoft-ui.css`) mediante hash SHA-256 idéntico.
- Se confirma presencia en runtime de reglas objetivo: `.mx-page-container.mx-page-container-full`, `.mx-showcase-hero`, `.mx-showcase-metrics`, `.mx-showcase-main-grid` y `.mx-showcase-main-surface-grid`.
- Se confirma que la otra hoja cargada (`_content/MudBlazor/MudBlazor.min.css`) no contiene esos selectores y no sobreescribe dichas reglas específicas.
- Resultado operativo: el runtime local queda alineado con el fuente vigente; no se aplican cambios adicionales de layout, páginas ni CSS en esta ejecución.

## 25) Actualización 2026-04-06 (UTC) — Regeneración completa de `MachSoft.UI.Showcase` como host limpio
- Se regenera `src/MachSoft.UI.Showcase` desde cero para eliminar deuda visual/estructural acumulada y mantener un host pequeño, gobernable y auditable.
- `Layout/MainLayout.razor` se consolida como único layout global y único consumidor estructural de `MxWorkspaceLayout` (sin `ShowcasePageLayout` ni sublayouts equivalentes).
- El estado inicial contractual del shell queda explícito: `MainMenuOpen=false`, `LeftSidebarMode/RightSidebarMode=Overlay`, `LeftSidebarOpen/RightSidebarOpen=false`, con `MainContent` dominante al inicio.
- La navegación global se reduce a secciones clave (`Home`, `Foundations`, `Components`) mediante `NavigationMenu` dedicado y separado de sidebars funcionales.
- Se rehacen desde cero las páginas `Index`, `Foundations/Colors` y `Components/Buttons` con render directo en `@Body`, uso de `MxPageContainer FullWidth="true"` y composición central amplia.
- Se crea una capa de estilos local de showcase (`wwwroot/showcase.css`) sin duplicados heredados y con comportamiento responsive explícito para preservar dominancia del contenido principal.

## 26) Actualización 2026-04-07 (UTC) — Regeneración integral de `MachSoft.UI.Showcase` y reincorporación a solución
- Se reconstruye completamente `src/MachSoft.UI.Showcase` como host Blazor WebAssembly limpio con estructura base auditable (`Program`, `App`, `MainLayout`, `NavMenu`, `Pages`, `wwwroot`).
- `MainLayout.razor` queda como único layout global y único consumidor de `MxWorkspaceLayout`; no se incluye `ShowcasePageLayout` ni sublayouts equivalentes.
- El estado inicial contractual del shell se mantiene explícito: `MainMenuOpen=false`, `NavigationMenu` cerrado, `LeftSidebarMode/RightSidebarMode=Overlay`, `LeftSidebarOpen/RightSidebarOpen=false`.
- Los controles globales del host se centralizan en el menú del header del shell (tema, navegación y sidebars).
- Se rehace catálogo por página con rutas activas para home, foundations/colors y componentes clave (`buttons`, `textfields`, `alerts`, `table`).
- Se consolida capa de estilos local única en `src/MachSoft.UI.Showcase/wwwroot/showcase.css` sin duplicados.
- Se agrega `src/MachSoft.UI.Showcase/MachSoft.UI.Showcase.csproj` a `MachSoft.UiPlatform.sln` para ejecución directa desde Visual Studio y validación en pipelines.


## 27) Actualización 2026-04-07 (UTC) — Base oficial de aplicación (Server + WebAssembly)
- Se consolida el patrón oficial de host sobre `MxWorkspaceLayout` para `MachSoft.Template.Server` y `MachSoft.Template.Wasm` con rutas base `/login`, `/` y `/work`.
- `MainLayout` se mantiene como único layout global en ambos hosts, con navegación global en `NavigationMenu`, acciones operativas en `LeftSidebar`, contenido de negocio en `MainContent` y panel contextual en `RightSidebar`.
- Se eliminan rutas heredadas de muestra (`/form`, `/list`, `/formulario`, `/lista`) para reducir ruido y asegurar foco exclusivo en patrones base de aplicación.
- Se implementa comportamiento contextual real en `/work`: selección en grid abre `RightSidebar` y permite cierre controlado.
- Se agrega guía operativa `docs/APP_BOOTSTRAP_GUIDE.md` para crear aplicaciones nuevas desde cero (Server y WebAssembly) sin prueba/error.
- Se ajustan smoke tests para validar rutas oficiales en ambos hosts (`/`, `/login`, `/work`).


## 28) Actualización 2026-04-07 (UTC) — Demos explícitas Server/WebAssembly
- Se agregan proyectos ejecutables explícitos `MachSoft.Demo.Server` y `MachSoft.Demo.WebAssembly` como referencia operativa separada de templates.
- Se incorpora ambos proyectos a `MachSoft.UiPlatform.sln` para build/test unificado y validación continua.
- Se mantiene paridad funcional con templates: `MainLayout` único con `MxWorkspaceLayout`, rutas `/`, `/login`, `/work`, rail operativo izquierdo y panel contextual derecho controlado por selección.
- Smoke tests amplían cobertura a 4 hosts (`template-server`, `template-wasm`, `demo-server`, `demo-wasm`) validando apertura de rutas oficiales.

## 29) Actualización 2026-04-07 (UTC) — Corrección estricta de patrón visual base en `/`, `/login`, `/work`
- Se rehacen las tres rutas base en los cuatro hosts activos (`Template.Server`, `Template.Wasm`, `Demo.Server`, `Demo.WebAssembly`) para alinear composición real con el patrón enterprise solicitado.
- `/` pasa a ser pantalla operativa de entrada con mosaico responsive homogéneo (iconografía técnica + nombre de módulo), centrado y sin hero de marketing/showcase.
- `/login` migra a layout dedicado (`LoginLayout`) para separar completamente la pantalla de acceso del shell normal: panel visual institucional + panel de formulario con jerarquía explícita.
- `/work` adopta composición de trabajo estable con rail operativo estrecho, barra de acciones + búsqueda en región principal, grid dominante y panel contextual derecho con apertura/cierre explícito por selección.
- Se incorpora control de tema visible y accionable en header del shell (y también en login dedicado) para asegurar gobernanza global del tema fuera del menú técnico.
- Se actualiza la capa CSS base `machsoft-ui.css` para fijar proporciones enterprise, estabilidad del body central y respuesta adaptativa en breakpoints.

## 30) Actualización 2026-04-07 (UTC) — Alineación de `/work` con guía estructural Figma (sin romper autoridad visual DS)
- Se toma `docs/benchmark/GuiasFigma/VistaTipoGrid.*` exclusivamente como referencia de estructura/composición y se conserva `MachSoft.DesignSystem` como fuente de verdad visual final.
- Se introduce cabecera operativa en `/work` (overline + título + búsqueda + acción circular) para replicar jerarquía y distribución horizontal de la guía de layout.
- Se refactoriza el rail izquierdo para usar `MxIconButton` en lugar de `MudIcon` directo en páginas de negocio, manteniendo API pública `Mx*`.
- Se mantiene región central dominante con `MxDataGrid` y metadatos de paginación/volumen, preservando proporciones tipo “grid-focused workspace”.
- Se consolida panel contextual derecho como card de detalle colapsable, con ancho reducido cuando no hay selección y expansión por interacción real.
- Se aplica la misma composición a los cuatro hosts activos (`Demo.Server`, `Demo.WebAssembly`, `Template.Server`, `Template.Wasm`) para mantener paridad operativa entre demos y plantillas.

## 31) Actualización 2026-04-07 (UTC) — Contrato de anchos configurables para `LeftSidebar`/`RightSidebar`
- Se consolida el contrato del shell para que `LeftSidebarWidth` y `RightSidebarWidth` controlen el ancho real de paneles laterales visibles en `Inline` y `Overlay`.
- Se elimina hardcode redundante de anchos laterales en CSS base de `MxWorkspaceLayout`; el layout consume anchos vía variables provenientes del contrato público.
- Se actualizan hosts (`Template` y `Demo`, Server + WebAssembly) para exponer ambos anchos desde `MainLayout`, con valores por defecto trazables y fáciles de ajustar para validación visual.
- Se incorpora región `RightSidebar` operativa en hosts para validar visualmente el ancho derecho y su impacto estructural en `MainContainer` cuando aplica modo `Inline`.

## 32) Actualización 2026-04-07 (UTC) — Vista `/work` full-body con sidebars funcionales en overlay
- Se ajusta la composición de `/work` (Demo/Template en Server + WebAssembly) para que el `MxDataGrid` ocupe el cuerpo completo de `MainContent`, eliminando rail y panel contextual embebidos dentro de la página.
- Se reasigna la responsabilidad funcional al shell: acciones operativas en `LeftSidebar` y contexto de selección en `RightSidebar`, ambos en modo `Overlay` para no consumir ancho estructural del grid.
- Se compacta el header operativo a altura efectiva de 48px con controles iconográficos (acciones, selección y tema), mejorando comportamiento del cambio de tema en vistas centradas en grid.
- Se mantiene gobernanza visual del Design System en `machsoft-ui.css` sin exponer contratos `Mud*` fuera de implementación interna.

## 33) Actualización 2026-04-07 (UTC) — Corrección UX purista de shell para vistas tipo grid
- Se formaliza el comportamiento solicitado para `/work` en los cuatro hosts activos (Template + Demo, Server + WebAssembly): el `MxDataGrid` ocupa todo el body central y no se incrustan paneles internos de acciones/contexto dentro de la página.
- `LeftSidebar` pasa a modo `Inline` siempre visible en `/work` con ancho contractual de `48px`, quedando dedicado al rail de acciones operativas persistentes.
- `RightSidebar` se mantiene en `Overlay` y su apertura/cierre queda gobernada por la selección real de registros del grid; al seleccionar se abre automáticamente y al limpiar selección se cierra.
- Se agrega estado compartido de shell para selección (`WorkSelectionActive`, conteo, título y metadatos) para sincronizar página de grid y layout sin romper contratos públicos `Mx*`.
- Se ajusta el header para priorizar estabilidad visual en modo grid y corregir contraste en tema (subtitle + acción de tema), manteniendo altura de 48px y consistencia con tokens del Design System.

## 34) Actualización 2026-04-07 (UTC) — Corrección de sidebars en `/work` y neutralidad visual de overlays
- Se corrige `MxWorkspaceLayout` para que la apertura de sidebars en modo `Overlay` no desplace `MainContent`; el contenido principal permanece fijo y los paneles se superponen como capa modal con backdrop.
- Se actualiza la política de `/work` en los cuatro hosts activos (`Demo.Server`, `Demo.WebAssembly`, `Template.Server`, `Template.Wasm`): `RightSidebar` pasa a `MxSidebarMode.Inline` para exponer detalle contextual persistente al seleccionar registros.
- Se elimina dependencia de `RightSidebarOpen` ligada a selección en `/work`; la selección ahora sólo actualiza contenido contextual en el panel inline sin transición overlay.
- Se ajusta documentación contractual y pruebas bUnit para reflejar la regla oficial: overlays no alteran posición visual del `MainContainer`.

## 35) Actualización 2026-04-08 (UTC) — Reapertura contextual de `RightSidebar` por selección + sincronización global de tema
- Se ajusta la política de `/work` en los cuatro hosts activos (`Demo.Server`, `Demo.WebAssembly`, `Template.Server`, `Template.Wasm`) para que `RightSidebar` opere nuevamente en `Overlay` y mantenga estado inicial cerrado (`RightSidebarOpen=false`).
- Se formaliza regla operativa solicitada: `RightSidebarOpen` se activa **si y solo si** existe al menos un registro seleccionado en el grid (`WorkSelectionActive=true`), y se cierra automáticamente al limpiar selección o salir de `/work`.
- Se corrige la propagación de cambio de tema en layouts de login de todos los hosts: el toggle ahora refleja `ShellState.DarkMode` en tiempo real y se suscribe al evento `ShellState.Changed` para mantener consistencia visual en todo el sitio.
- Se preserva contrato de Design System y shell sin exposición de API `Mud*` en contratos públicos `Mx*`.

## 36) Actualización 2026-04-08 (UTC) — Consolidación de patrón operativo único sobre `MxWorkspaceLayout`
- Se consolida `MxWorkspaceLayout` como única fuente estructural para páginas operativas y se formaliza el patrón semántico obligatorio: `NavigationMenu` global, `LeftSidebar` funcional, `MainContent` dominante y `RightSidebar` contextual.
- Se incorpora en el contrato del layout el modo responsive contextual: cuando `RightSidebar` está en `Overlay` y abierto, en mobile/tablet reemplaza visualmente la región central y expone acción explícita de cierre contextual.
- Se actualiza documentación contractual (`docs/components/MxWorkspaceLayout.md`) para declarar que `MxWorkspaceLayout` reemplaza conceptualmente el rol del benchmark `MlxMainContainer`, sin replicar componente, nombres ni estilos del origen.
- Se refactoriza `/work` (Demo/Template en Server + WebAssembly) como implementación de referencia reutilizable: toolbar operativa + listado principal en `MainContent`, manteniendo sidebars funcionales gobernadas por el shell.
- Se amplía cobertura de pruebas para verificar estado responsive contextual, acción de cierre del panel derecho y ausencia de layout tripartito duplicado dentro de `Work.razor`.

## 37) Actualización 2026-04-08 (UTC) — Ajuste de composición operativa real (Users/Designer/Warehouse como referencia)
- Se corrige la composición de `/work` en los cuatro hosts (`Demo.Server`, `Demo.WebAssembly`, `Template.Server`, `Template.Wasm`) manteniendo arquitectura vigente (`MxWorkspaceLayout` único) y reforzando patrón operativo observado en benchmark: bloque superior de trabajo + barra de acciones de listado + grid dominante.
- Se rehace el bloque superior para separar jerarquía de operación (título/modo), contexto visible y capa de acciones de vista/listado, evitando encabezado plano sin transición hacia el grid.
- Se refuerza `LeftSidebar` como rail operativo estable mediante agrupación explícita de acciones (modo vs acciones), separación visual y ritmo vertical consistente.
- Se robustece `RightSidebar` contextual con cabecera propia, estado contextual visible y acciones de detalle para mantener balance con el peso visual del área principal.
- Se actualiza la hoja base `machsoft-ui.css` para reflejar esta densidad operativa (espaciados, jerarquía y barras intermedias) sin importar estilos/tokens del benchmark origen.

## 38) Actualización 2026-04-08 (UTC) — `/work` como referencia operativa fiel al patrón origen (sin alterar arquitectura)
- Se refuerza `/work` en los cuatro hosts activos (`Demo.Server`, `Demo.WebAssembly`, `Template.Server`, `Template.Wasm`) para materializar explícitamente el flujo operativo `browse/detail/edit/new` con estado compartido y trazable.
- Se mantiene `MxWorkspaceLayout` como única fuente estructural: `NavigationMenu` global, `LeftSidebar` funcional, `MainContent` dominante y `RightSidebar` contextual; no se incorpora layout tripartito adicional dentro de la página.
- `LeftSidebar` evoluciona a rail operativo real (no residual): agrupación de acciones por contexto, jerarquía visual por estado activo/deshabilitado y ritmo vertical consistente.
- `MainContent` se ordena con cabecera de trabajo + barra de acciones de listado + superficie dominante de grid, añadiendo transición visible para modo `new` sin romper la centralidad del listado.
- `RightSidebar` se consolida como inspector contextual con cabecera, estado de modo/selección y acciones de continuidad (`detalle`/`edición`) vinculadas a la selección del grid.
- Se extiende `ShellState` con `WorkMode` para sincronizar modo operativo entre página y sidebars, preservando apertura contextual responsive del panel derecho en `Overlay` cuando aplica contexto real.

## 39) Actualización 2026-04-09 (UTC) — Normalización visual de `/work` para lectura operativa natural
- Se mantiene intacta la arquitectura vigente (`MxWorkspaceLayout` + regiones `LeftSidebar`, `MainContent`, `RightSidebar`) en los cuatro hosts activos (`Demo.Server`, `Demo.WebAssembly`, `Template.Server`, `Template.Wasm`) sin crear layouts alternativos.
- Se depuran rótulos didácticos/excesivos en `/work` para evitar lectura de maqueta y reforzar lenguaje de producto real: cabecera sobria, toolbar integrada y panel contextual con copy funcional.
- `LeftSidebar` se simplifica como rail operativo contextual (modos + acciones rápidas) sin encabezados narrativos de arquitectura.
- `RightSidebar` conserva peso contextual y acciones de continuidad, removiendo etiquetado explícito tipo “inspector” y priorizando información del registro activo.
- Se ajusta `machsoft-ui.css` para acompañar la iteración (densidad, jerarquía y acabado visual) manteniendo autoridad del Design System MachSoft.

## 38) Actualización 2026-04-09 (UTC) — Traducción fiel de `/work` al patrón benchmark usando contratos MachSoft
- Se refactoriza `/work` en los cuatro hosts activos (`Template.Server`, `Template.Wasm`, `Demo.Server`, `Demo.WebAssembly`) para acercar la composición al benchmark origen (rail izquierdo operativo, grid dominante central y panel contextual derecho) sin duplicar layout tripartito dentro de `MainContent`.
- Se mantiene `MxWorkspaceLayout` como única fuente de verdad estructural del shell; no se introduce clon de `MlxMainContainer` ni layouts paralelos.
- Se sustituyen controles manuales en la experiencia de `/work` por contratos `Mx*` (acciones con `MxButton`/`MxIconButton`, encabezado con `MxEntityHeader`, estados/contexto con `MxTag`, superficies con `MxSectionCard`, formulario de alta con `MxTextField` y listado con `MxDataGrid`).
- Se refuerza comportamiento de estados `browse/detail/edit/new` sincronizados entre página y shell, preservando apertura contextual del panel derecho basada en selección real del grid.
- Se ajusta la capa visual de `machsoft-ui.css` para evitar hardcodes de color en el área operativa y consolidar estilos gobernados por tokens `--mx-*`/superficies del Design System.
- Resultado operativo: `/work` queda más cercana al flujo del benchmark (Users/Warehouse/Designer) pero implementada con apariencia y contratos MachSoft, compatible con theming y gobernada por `MxWorkspaceLayout`.

## 40) Actualización 2026-04-09 (UTC) — Corrección de regresión de modo/selección y realineación estricta de `/work`
- Se corrige la regresión funcional en `Work.razor` (Demo/Template, Server + WebAssembly): `NormalizeMode` ahora acepta `null`/vacío de forma segura y evita `ToLowerInvariant()` sobre valores nulos.
- Se blinda la transición de modo para cortar recursión entre `SetMode`, `OnShellStateChanged`, `ShellState.SetWorkMode`, `SetWorkSelection` y `ClearWorkSelection`: no se reemite modo cuando ya está aplicado y no se limpia selección al sincronizar eventos del shell.
- Se mantiene `MxWorkspaceLayout` como arquitectura base y se refuerza la estructura operativa solicitada del benchmark en `/work`: rail izquierdo estrecho operativo, región central dominante con `MxDataGrid` + toolbar real, y panel contextual derecho ligado a selección.
- Se elimina composición narrada/no funcional en `/work` y `LeftSidebar`; el rail opera con acciones compactas usando únicamente contratos `Mx*` y clases `mx-*`.
- Se ajusta `machsoft-ui.css` para soportar densidad del rail compacto con tokens del Design System (sin hardcodes fuera del sistema).

## 41) Actualización 2026-04-09 (UTC) — `/work` reanclado a benchmark operativo `Users` + corrección de regresión de modo
- Se corrige la regresión funcional en `Work.razor` de los cuatro hosts activos (`Template.Server`, `Template.Wasm`, `Demo.Server`, `Demo.WebAssembly`): `NormalizeMode(string?)` tolera `null`/vacío sin riesgo de `ToLowerInvariant()` sobre valor nulo.
- `SetMode` deja de reemitir sincronización al shell cuando el modo solicitado ya coincide con el estado actual, evitando ruido de eventos y recursión indirecta.
- Se desacopla el ciclo entre modo y selección (`SetMode` ↔ `OnShellStateChanged` ↔ `SetWorkSelection`/`ClearWorkSelection`) con guardas de sincronización y comparación previa de estado para no publicar cambios idénticos.
- Se rehace la composición central de `/work` para respetar el benchmark `Users`: toolbar operativa explícita, grid dominante en browse/detail/edit y transición a formulario en `new`, manteniendo el detalle contextual en `RightSidebar` y acciones en `LeftSidebar` sobre `MxWorkspaceLayout`.
- Toda la implementación usa contratos MachSoft (`Mx*`) y estilos `mx-*`, preservando theming/tokens del Design System sin introducir layout alternativo al shell oficial.

## 42) Actualización 2026-04-09 (UTC) — Consolidación final de `/work` contra benchmark `Users`
- Se valida en esta ejecución la paridad operativa de `/work` (cuatro hosts) con patrón `Users`: rail izquierdo contextual, toolbar de listado, grid central dominante, selección que habilita `detail/edit` y modo `new` con formulario dedicado.
- Se confirma endurecimiento funcional de modo/selección para eliminar regresiones por reemisión redundante y sincronización circular con estado de shell.
- Se mantiene cumplimiento arquitectónico: `MxWorkspaceLayout` como único contenedor estructural y consumo exclusivo de contratos `Mx*` en la superficie de negocio.

## 43) Actualización 2026-04-09 (UTC) — Aplicación exacta de `Work.razor` base en los 4 hosts
- Se aplica de forma homogénea la misma implementación de `Work.razor` (estructura + estilos inline asociados) en `Demo.Server`, `Demo.WebAssembly`, `Template.Server` y `Template.Wasm`, ajustando únicamente el `@using` de `ShellState` por host para compilar con servicios reales.
- Se conserva `MxWorkspaceLayout` como layout único y se preservan explícitamente las tres regiones operativas: `LeftSidebar` funcional, `MainContent` con `MxDataGrid` dominante y `RightSidebar` contextual por selección.
- Se mantiene el patrón solicitado de componentes MachSoft en superficie pública (`MxButton`, `MxDataGrid`, `MxSectionCard`, `MxTag`, `MxTextField`) sin volver a rail iconográfico ni exponer contratos `Mud*`.
- Se conserva la lógica anti-recursión de sincronización modo/selección (`SetMode` + `OnShellStateChanged` + guardas de supresión) para evitar loops de eventos con `ShellState`.
- Resultado operativo: los cuatro hosts quedan alineados sobre el mismo flujo funcional de `/work` con panel derecho contextual activo y paridad estructural verificable.
