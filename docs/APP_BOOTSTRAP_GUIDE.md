# APP_BOOTSTRAP_GUIDE

Guía cerrada para crear una app nueva sobre `MachSoft.UI` con patrón oficial `/login`, `/` y `/work`.

## 1) Referencias y paquetes mínimos

### Server (`Microsoft.NET.Sdk.Web`)
1. Referenciar proyecto/paquete `MachSoft.UI`.
2. En `Program.cs` registrar:
   - `builder.Services.AddRazorComponents().AddInteractiveServerComponents();`
   - `builder.Services.AddMachSoftUi();`
   - estado de shell (ej. `ShellState`) en `Scoped`.

### WebAssembly (`Microsoft.NET.Sdk.BlazorWebAssembly`)
1. Referenciar proyecto/paquete `MachSoft.UI`.
2. En `Program.cs` registrar:
   - `builder.Services.AddMachSoftUi();`
   - estado de shell (ej. `ShellState`) en `Singleton`.

## 2) `MainLayout` oficial (único layout global)

Requisitos obligatorios:
- raíz: `MxWorkspaceLayout`;
- `@Body` dentro de `MainContent`;
- `NavigationMenu` para navegación global;
- `LeftSidebar` y `RightSidebar` como regiones del shell;
- sin sublayouts ni wrappers que recompongan toda la página.

Estado inicial de shell:
- `MainMenuOpen = false`
- `LeftSidebarMode = Overlay`
- `RightSidebarMode = Overlay`
- `LeftSidebarOpen = false`
- `RightSidebarOpen = false`

## 3) Navegación global mínima

Definir en `NavigationMenu`:
- Home (`/`)
- Login (`/login`)
- Work (`/work`)

## 4) Controles globales en menú del shell

El menú Shell del header debe incluir:
- cambio tema light/dark;
- abrir/cerrar `NavigationMenu`;
- modo `LeftSidebar`;
- modo `RightSidebar`;
- abrir/cerrar overlay izquierdo;
- abrir/cerrar overlay derecho.

## 5) Páginas base obligatorias

### `/login`
- branding institucional;
- campos: usuario, contraseña, idioma;
- acciones: limpiar / aceptar;
- composición limpia, enterprise y estable.

### `/`
- `MxPageContainer FullWidth="true"`;
- mosaico de accesos rápidos a módulos;
- sin hero de marketing y sin catálogo técnico.

### `/work`
- rail izquierdo de acciones operativas (región `LeftSidebar`);
- región central dominante con acciones y búsqueda;
- grid/listado como núcleo principal;
- panel contextual en `RightSidebar` que abre al seleccionar un registro y se cierra de forma controlada.

## 6) Estilos base

1. Cargar `/_content/MachSoft.UI/machsoft-ui.css`.
2. Usar clases utilitarias de app base (`mx-app-*`) para spacing y composición.
3. No duplicar CSS ni mezclar capas de estilo heredadas del Showcase.

## 7) Diferencias Server vs WebAssembly

- **Server**: estado por circuito (`Scoped`), render interactivo server.
- **WASM**: estado en cliente (`Singleton`), mismo patrón visual/funcional.
- Contrato visual y semántico del shell debe ser idéntico en ambos hosts.

## 8) Errores típicos de arranque y prevención

1. **Error**: segundo layout o sublayout en páginas.
   - **Evitar**: dejar `MainLayout` como único layout global.
2. **Error**: sidebars abiertos por defecto.
   - **Evitar**: inicializar overlay + cerrado.
3. **Error**: controles globales puestos dentro del body.
   - **Evitar**: mantener controles en menú Shell del header.
4. **Error**: `/work` sin comportamiento contextual.
   - **Evitar**: abrir `RightSidebar` al seleccionar fila y cerrar explícitamente.

## 9) Estructura mínima recomendada

- `Layout/MainLayout.razor`
- `Pages/Index.razor` (`/`)
- `Pages/Login.razor` (`/login`)
- `Pages/Work.razor` (`/work`)
- `Services/ShellState.cs`
- `Services/WorkModels.cs`

## 10) Checklist de validación inicial

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
4. Verificar en Server y WASM:
   - `/login` abre sin excepción y renderiza;
   - `/` abre sin excepción y renderiza;
   - `/work` abre sin excepción, renderiza y abre/cierra panel contextual.
