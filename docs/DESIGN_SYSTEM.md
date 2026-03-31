# MachSoft Design System (Normativo Oficial)

## 1) Propósito
Definir y gobernar la identidad visual oficial de MachSoft para todos los productos UI basados en esta plataforma.

Este documento es **contractual**: si existe conflicto entre una implementación y este documento, prevalece este documento.

## 2) Principios visuales oficiales
- Enterprise técnico.
- Limpio, preciso, sobrio y robusto.
- Jerarquía por color institucional, tipografía, borde, superficie y spacing.
- Sin decoración gratuita.

## 3) Color principal oficial
- **Primary oficial:** `#005098`.
- Uso obligatorio: marca, acción primaria, navegación activa, foco principal, links y headers institucionales.

## 4) Acento técnico
- **Technical accent:** `#00AFCF`.
- Uso permitido: highlight controlado, soporte de interacción y acento opcional en visualización de datos.
- Uso prohibido: color dominante o reemplazo del primary institucional.

## 5) Contraste oficial
- Alto, frío y controlado.
- Priorizar legibilidad en tablas, formularios y workflows.

## 6) Neutral palette oficial
- Light neutrals: `#F4F7FB`, `#E9EEF5`, `#D8E0EA`, `#C0CBD9`, `#96A5B8`, `#73849A`, `#55667B`, `#3D4B5D`, `#243142`, `#121B28`.
- Dark neutrals: `#0D1522`, `#131F31`, `#1A2940`, `#2C3D56`.

## 7) Semantic colors
- Info: `#0068C9`
- Success: `#1B7A48`
- Warning: `#B06A00`
- Error: `#B4232D`

## 8) Typography scale
- Familia: `Inter`, fallback `Segoe UI`, `sans-serif`.
- Escala operativa: 12, 14, 16, 18, 20, 24, 30, 36 px.
- Line-height: 1.2–1.55 según nivel.
- Botones y labels con peso 600; body orientado a lectura continua.

## 9) Spacing scale
- Sistema 4px-based: 4, 8, 12, 16, 20, 24, 32, 40, 48, 64 px.
- Densidad objetivo: compacta/operativa (enterprise), nunca hacinada.

## 10) Radius policy
- Base: **4px** y **8px**.
- Excepción destacada: **16px** (cards o contenedores específicos).
- Prohibido: lenguaje predominantemente redondeado/blando.

## 11) Stroke policy
- Base: **1px** y **2px**.
- Énfasis excepcional: **4px**.
- Bordes/divisores deben verse técnicos, limpios y estables.

## 12) Elevation policy
- Prioridad en borde + superficie.
- Sombras contenidas y cortas.
- Niveles operativos: `none`, `elevation-1`, `elevation-2`; `elevation-3` solo casos de destaque.

## 13) Surface model
- Superficies planas o plano-elevadas.
- Modo base claro, frío y sobrio.
- Sin glassmorphism, sin blur ornamental, sin sombras excesivas.

## 14) Iconography direction
- Lineal, geométrica, técnica y de bajo ruido.
- Preferencia outline o peso controlado.
- Evitar iconografía cartoon/rounded-friendly/cargada.

## 15) Interaction states
- Hover: variación controlada del tono principal (`PrimaryHover`).
- Active: tono más profundo (`PrimaryActive`).
- Disabled: contraste reducido sobre neutral surface.
- Error/Warning/Success/Info semánticos con contraste AA mínimo.

## 16) Focus ring policy
- Focus visible obligatorio en componentes interactivos.
- Anillo principal: derivado de `#005098`.
- En dark mode puede usar contraste complementario con acento técnico.

## 17) Dark mode policy
- Debe conservar jerarquía institucional (primary sigue siendo eje).
- Contraste legible y consistente en navegación, formularios y tablas.
- No invertir decisiones de forma/radius/spacing entre modos.

## 18) Lo que NO pertenece al lenguaje MachSoft
- Estética Material genérica como identidad final.
- UI consumer/juguetona o con exceso de redondeo.
- Hardcode de color fuera de tokens (salvo excepción justificada y documentada).
- Adornos visuales no funcionales.
- Exposición de MudBlazor como marca visual del producto.

## 19) Implementación obligatoria
- Tokens en `MachSoft.DesignSystem`.
- Theme bridge en `MachSoft.UI`.
- API pública solo `Mx*`.
- Showcase como referencia viva del sistema.
