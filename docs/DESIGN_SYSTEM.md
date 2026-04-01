# MachSoft Design System (Autoridad Oficial)

## Dirección visual vigente
- Base graphite / gris espacial.
- Acento cian controlado.
- Azul profundo como apoyo.
- Light/dark simétricos en lenguaje de superficie.

## Reglas de interacción adicionales (catálogo completo)
1. Todo control interactivo debe exponer foco visible y estados hover/active/disabled.
2. `MxDataGrid` usa superficie técnica: header contrastado, hover de fila, selección explícita y estados vacíos/carga/error.
3. `MxFileUpload` exige affordance fuerte de dropzone y estado drag-over distinguible en ambos temas.
4. `MxMultiSelect` debe mostrar seleccionados persistentes y permitir remoción individual.
5. `MxAutocomplete` debe ofrecer respuesta inmediata con límites de resultados y comportamiento coherente de clear.
6. Barras productivas (`MxCrudToolbar`, `MxFilterBar`, `MxEntityHeader`) priorizan legibilidad y densidad operativa.

## Política de contrato visual
- Prohibido entregar wrappers triviales sin estilado/integración MachSoft.
- Prohibido usar apariencia Material genérica como resultado final.
- Los componentes deben consumir tokens de `MachSoft.DesignSystem` vía `MachSoft.UI`.

## Gobernanza
- Cualquier cambio que amplíe catálogo debe reflejarse en `UI_FOUNDATION_CONTRACT.md`.
- Showcase y templates se consideran pruebas de integración visual y funcional.
