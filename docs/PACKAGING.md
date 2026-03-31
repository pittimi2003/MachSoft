# Empaquetado

Paquetes NuGet:
- `MachSoft.DesignSystem`
- `MachSoft.UI`

## Comando
- `./build/pack.sh`

El script ejecuta:
- `dotnet pack src/MachSoft.DesignSystem/MachSoft.DesignSystem.csproj -c Release --no-build`
- `dotnet pack src/MachSoft.UI/MachSoft.UI.csproj -c Release --no-build`

## Precondición
`pack` requiere que `build` haya generado binarios en `Release`; en flujo limpio ejecutar antes `restore` y `build`.

## Salida
- `artifacts/nuget`
