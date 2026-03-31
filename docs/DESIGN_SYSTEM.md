# MachSoft Design System (Autoridad Oficial)

## 1) Propósito del sistema
Definir una identidad única de producto para MachSoft: plataforma enterprise técnica, sobria y robusta. Este documento es contractual y prevalece sobre implementaciones puntuales.

## 2) Principios visuales oficiales
1. Base estructural en neutros graphite (no azul corporativo dominante).
2. Identidad y energía por accent cian controlado.
3. Azul profundo solo como apoyo institucional secundario.
4. Alto contraste frío, jerarquía clara y legibilidad sostenida.
5. Estética técnica madura: limpia, precisa, sin ornamento.

## 3) Base cromática oficial
- **Graphite estructural:** `#F3F6F8` → `#1A2530`.
- **Brand accent (principal):** `#00B5D8`.
- **Azul profundo secundario:** `#123E73`.

## 4) Brand accent principal
- Token base: `Accent = #00B5D8`.
- Variantes: `AccentHover = #0099BA`, `AccentActive = #007F99`, `AccentSoft = #D9F5FB`.
- Uso obligatorio: acción primaria, selección/activo, links, foco visual principal, highlights técnicos de interacción.

## 5) Neutral family (gris espacial/grafito)
- Familia oficial: `Graphite050/100/200/300/400/500/600/700/800/900`.
- Uso estructural: fondos, shells, navegación, headers, superficies, bordes, separadores, tablas y layout.

## 6) Azul profundo secundario
- Token: `DeepBlue = #123E73` (+ variantes).
- Uso permitido: soporte institucional, variantes sobrias y señalización secundaria.
- Uso prohibido: desplazar al graphite como base o al cian como identidad.

## 7) Contraste
- Contraste alto, frío, controlado y orientado a productividad de larga sesión.
- Jerarquía visual por color + tipografía + spacing + superficie + borde + estado.

## 8) Semantic colors
- Info `#1870D5`
- Success `#1F8A54`
- Warning `#B97810`
- Error `#C23844`

## 9) Typography scale
- Familia: `Inter`, fallback `Segoe UI`, `sans-serif`.
- Escala oficial: `12, 14, 16, 18, 20, 24, 30, 36 px`.
- Button/labels: peso 600.
- Body: line-height de lectura operativa (`1.5–1.55`).

## 10) Spacing scale
- Escala 4px-based: `4, 8, 12, 16, 20, 24, 32, 40, 48, 64 px`.
- Densidad objetivo: compacta y respirable para enterprise real.

## 11) Radius policy
- Operativo: `4px` y `8px`.
- Extensión controlada: `12px` y `16px` solo en contenedores puntuales.
- Prohibido: interfaz blanda o excesivamente redondeada.

## 12) Stroke policy
- Base: `1px` y `2px`.
- Énfasis excepcional: `3px` y `4px`.
- Bordes/divisores: técnicos, limpios y consistentes.

## 13) Elevation policy
- Prioridad: superficie + borde antes que sombra.
- Niveles: `none`, `elevation-1`, `elevation-2`, `elevation-3` solo para énfasis concreto.
- Sin sombras pesadas ni blur ornamental.

## 14) Surface model
- Superficies planas o plano-elevadas.
- Con borde visible cuando aporte jerarquía.
- Sin glassmorphism.

## 15) Iconography direction
- Lineal, geométrica, técnica, de trazo limpio y ruido bajo.
- Preferencia outline/peso controlado.

## 16) Interaction states
- Hover/Active del primario por escala de accent.
- Disabled con contraste reducido sobre neutros.
- Estados semánticos legibles en light y dark.

## 17) Focus ring policy
- Focus visible obligatorio en todo control interactivo.
- Ring base: accent (`#00B5D8`) con opacidad controlada.
- En dark: contraste incrementado (`FocusRingContrast`).

## 18) Light theme policy
- Claro, limpio, institucional y preciso.
- Graphite claro como estructura (`Background/Shell`), accent cian como señal principal.

## 19) Dark theme policy
- Premium técnico (control room), robusto, sin neones agresivos.
- Misma identidad de marca: misma tipografía, spacing, radios, estados y semántica.

## 20) Lo que NO forma parte del lenguaje MachSoft
- Material genérico como resultado visual final.
- Estética consumer/juguetona/decorativa.
- Cian saturado como “baño visual”.
- Azul profundo dominando toda la interfaz.
- Hardcode de color fuera de tokens (excepto casos mínimos justificados).
- Apps consumiendo `Mud*` directo como contrato público.

## Reglas de uso obligatorias
1. Apps de negocio consumen **solo `Mx*`**.
2. Autoridad visual: `MachSoft.DesignSystem` + `MachSoft.UI`.
3. Todo nuevo componente respeta tokens/theme/contrato oficial.
4. No introducir atajos visuales fuera de tokens/theme/librería.
