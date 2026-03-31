# Flujo Codex

1. Implementar historia en `MachSoft.UI` o template.
2. Asegurar uso de `Mx*`.
3. Actualizar showcase.
4. Añadir/ajustar pruebas.
5. Ejecutar validación técnica en este orden:
   - `restore`
   - `build`
   - `test`
   - instalación Playwright
   - smoke tests
   - arranque de showcase y templates
   - `pack`
6. Documentar cambios en ADR si altera contratos.

## Criterio de cierre en validación real
Si un paso queda bloqueado (por ejemplo `NU1301` por conectividad a feed NuGet), dejar diagnóstico explícito con:
- causa exacta,
- archivos afectados,
- corrección propuesta,
- impacto para inicio de historias de usuario.
