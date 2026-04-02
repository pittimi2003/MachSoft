# MachSoft — Contrato permanente de ejecución para Codex

Este archivo define reglas permanentes para cualquier trabajo sobre el catálogo MachSoft en este repositorio.

## 1) Principios no negociables
- No entregar mínimos funcionales como cierre de etapa.
- No introducir wrappers triviales sin valor de contrato o de diseño.
- No aceptar demos o prototipos como cierre operativo.
- MudBlazor se usa solo como implementación interna.
- La API pública para negocio debe exponerse con prefijo `Mx*`.
- El Design System MachSoft es la autoridad visual obligatoria.
- Todo cambio de componentes debe reflejarse en showcase auditable.
- La documentación operativa es obligatoria para cerrar trabajo.
- No se cierra ninguna ejecución con errores de Razor.
- No modificar archivos generados automáticamente.
- Validar siempre con build real del repositorio antes de cerrar.

## 2) Criterios de aceptación de entregas
Para considerar una entrega como válida deben cumplirse todos:
1. Build real en verde (sin errores de compilación ni de Razor).
2. Contratos públicos `Mx*` consistentes y sin exposición accidental de `Mud*`.
3. Evidencia de actualización de showcase cuando aplique.
4. Evidencia de actualización documental cuando cambie el estado operativo.

## 3) Gobernanza de continuidad
- El estado operativo del catálogo se gobierna en `docs/CATALOG_EXECUTION_STATUS.md`.
- Cualquier ejecución relevante que cambie el estado real del catálogo debe actualizar ese archivo en el mismo cambio.
- Si hay divergencia entre chat y repositorio, prevalece lo documentado en archivos versionados.

## 4) Restricciones de seguridad de cambios
- No “arreglar” generado tocando archivos autogenerados.
- Resolver en fuente de verdad (componentes base, contratos, design system, o configuración) y luego regenerar si corresponde.

## 5) Regla de cierre
No declarar cierre si falta al menos uno de estos puntos:
- build real ejecutado;
- estado del catálogo actualizado;
- decisiones/controles críticos trazables en documentos del repositorio.
