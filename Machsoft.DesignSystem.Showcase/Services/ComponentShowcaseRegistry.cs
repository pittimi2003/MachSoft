using Machsoft.DesignSystem.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Machsoft.DesignSystem.Showcase.Services;

public sealed class ComponentShowcaseRegistry
{
    private static readonly IReadOnlyDictionary<string, Func<Dictionary<string, object?>>> ParameterFactories = new Dictionary<string, Func<Dictionary<string, object?>>>(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(MxAlert)] = () => new() { ["Severity"] = Severity.Info, ["ChildContent"] = Text("Ejemplo de alerta") },
        [nameof(MxBadge)] = () => new() { ["Content"] = "3", ["ChildContent"] = Text("Notificaciones") },
        [nameof(MxBreadcrumbs)] = () => new() { ["Items"] = new List<BreadcrumbItem> { new("Inicio", href: "/"), new("Catálogo", href: "#") } },
        [nameof(MxButton)] = () => new() { ["ChildContent"] = Text("Botón de ejemplo") },
        [nameof(MxButtonFAB)] = () => new() { ["Icon"] = Icons.Material.Filled.Add },
        [nameof(MxCard)] = () => new() { ["ChildContent"] = Text("Contenido de tarjeta") },
        [nameof(MxChip)] = () => new() { ["ChildContent"] = Text("Chip") },
        [nameof(MxConfirmDialogButton)] = () => new() { ["ButtonText"] = "Confirmar" },
        [nameof(MxDatePicker)] = () => new() { ["Label"] = "Fecha" },
        [nameof(MxDateRangePicker)] = () => new() { ["Label"] = "Rango" },
        [nameof(MxDesignSystemProvider)] = () => new() { ["ChildContent"] = Text("Proveedor activo") },
        [nameof(MxFocusTrap)] = () => new() { ["ChildContent"] = Text("Zona con foco") },
        [nameof(MxForm)] = () => new() { ["ChildContent"] = Text("Formulario básico") },
        [nameof(MxHotkey)] = () => new() { ["Key"] = "ctrl+s", ["ChildContent"] = Text("Atajo activo") },
        [nameof(MxIcon)] = () => new() { ["Icon"] = Icons.Material.Filled.CheckCircle },
        [nameof(MxIconButton)] = () => new() { ["Icon"] = Icons.Material.Filled.Settings, ["AriaLabel"] = "Configuración" },
        [nameof(MxImage)] = () => new() { ["Src"] = "https://placehold.co/320x180/png", ["Alt"] = "Imagen demo" },
        [nameof(MxLink)] = () => new() { ["Href"] = "#", ["ChildContent"] = Text("Enlace") },
        [nameof(MxNavigationShell)] = () => new() { ["MainContent"] = Text("Contenido principal") },
        [nameof(MxNumericField)] = () => new() { ["Label"] = "Cantidad", ["Value"] = 5m },
        [nameof(MxPaper)] = () => new() { ["ChildContent"] = Text("Paper container") },
        [nameof(MxPopover)] = () => new() { ["AnchorContent"] = Text("Ancla"), ["ChildContent"] = Text("Contenido") },
        [nameof(MxProgress)] = () => new() { ["Value"] = 55d },
        [nameof(MxRadioGroup)] = () => new() { ["Label"] = "Prioridad", ["Options"] = new[] { "Alta", "Media", "Baja" }, ["Value"] = "Media" },
        [nameof(MxSelect)] = () => new() { ["Label"] = "Selecciona", ["Items"] = new[] { "Uno", "Dos", "Tres" }, ["Value"] = "Uno" },
        [nameof(MxSplitPanel)] = () => new() { ["Left"] = Text("Panel izquierdo"), ["Right"] = Text("Panel derecho") },
        [nameof(MxSwitch)] = () => new() { ["Label"] = "Activar" },
        [nameof(MxSwipeArea)] = () => new() { ["ChildContent"] = Text("Desliza aquí") },
        [nameof(MxTextField)] = () => new() { ["Label"] = "Nombre", ["Value"] = "Ejemplo" },
        [nameof(MxTimePicker)] = () => new() { ["Label"] = "Hora" },
        [nameof(MxTooltip)] = () => new() { ["Text"] = "Tooltip", ["ChildContent"] = Text("Pasa el cursor") },
    };

    private static readonly string[] CertificationChecks =
    [
        "Compila sin errores en solución completa",
        "Tiene ejemplo funcional en Showcase",
        "El ejemplo renderiza sin excepción",
        "Cubre 5 casos básicos válidos",
        "No rompe con configuración y sin parámetros ocultos"
    ];

    private static readonly ComponentShowcaseItem[] GenericItems =
    [
        new("datagrid", "MxDataGrid", typeof(Components.Examples.MxDataGridShowcaseExample), BuildMarkup("MxDataGrid"), BuildParameters("MxDataGrid"), CertificationChecks),
        new("simpletable", "MxSimpleTable", typeof(Components.Examples.MxSimpleTableShowcaseExample), BuildMarkup("MxSimpleTable"), BuildParameters("MxSimpleTable"), CertificationChecks),
        new("treeview", "MxTreeView", typeof(Components.Examples.MxTreeViewShowcaseExample), BuildMarkup("MxTreeView"), BuildParameters("MxTreeView"), CertificationChecks),
        new("fileupload", "MxFileUpload", typeof(Components.Examples.MxFileUploadShowcaseExample), BuildMarkup("MxFileUpload"), BuildParameters("MxFileUpload"), CertificationChecks),
    ];

    public IReadOnlyList<ComponentShowcaseItem> Items { get; }

    public ComponentShowcaseRegistry()
    {
        var standardItems = typeof(MxButton).Assembly
            .GetTypes()
            .Where(t => t is { IsPublic: true, IsAbstract: false } &&
                        t.Namespace == "Machsoft.DesignSystem.Components" &&
                        t.Name.StartsWith("Mx", StringComparison.Ordinal) &&
                        typeof(IComponent).IsAssignableFrom(t) &&
                        !t.ContainsGenericParameters)
            .Select(CreateItem);

        Items = standardItems
            .Concat(GenericItems)
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .OrderBy(item => item.Name)
            .ToArray();
    }

    public ComponentShowcaseItem? Find(string? key)
        => key is null ? null : Items.FirstOrDefault(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));

    private static ComponentShowcaseItem CreateItem(Type componentType)
    {
        var key = componentType.Name[2..].ToLowerInvariant();
        return new ComponentShowcaseItem(key, componentType.Name, componentType, BuildMarkup(componentType.Name), BuildParameters(componentType.Name), CertificationChecks);
    }

    private static string BuildMarkup(string componentName)
        => ParameterFactories.ContainsKey(componentName)
            ? $"<{componentName}>\n    <!-- ejemplo configurable -->\n</{componentName}>"
            : $"<{componentName} />";

    private static IDictionary<string, object> BuildParameters(string componentName)
        => ParameterFactories.TryGetValue(componentName, out var factory)
            ? factory().Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value!)
            : new Dictionary<string, object>();

    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
}

public sealed record ComponentShowcaseItem(
    string Key,
    string Name,
    Type Type,
    string SampleCode,
    IDictionary<string, object> Parameters,
    IReadOnlyList<string> CertificationChecks);
