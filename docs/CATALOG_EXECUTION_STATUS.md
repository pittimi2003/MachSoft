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
- **Fecha:** 2026-04-03 (UTC).
- **Tipo:** evolución contractual de layout reusable de plataforma.
- **Resultado:** shell oficial `MxWorkspaceLayout` implementado y consumido por Showcase/Template.* con validación técnica.
- **Impacto operativo:** layout de workspace unificado, reusable y gobernado por plataforma.

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
