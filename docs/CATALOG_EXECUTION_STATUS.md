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
