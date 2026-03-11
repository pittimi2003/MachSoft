using System.Reflection;
using Microsoft.AspNetCore.Components;
using Machsoft.DesignSystem.Components;

namespace Machsoft.DesignSystem.Showcase.Services;

public sealed class ComponentShowcaseRegistry
{
    private static readonly Dictionary<string, Func<Dictionary<string, object?>>> ParameterFactories = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(MxAlert)] = () => new()
        {
            ["Severity"] = MudBlazor.Severity.Info,
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Ejemplo de alerta informativa"))
        },
        [nameof(MxButton)] = () => new()
        {
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Botón de ejemplo"))
        },
        [nameof(MxCard)] = () => new()
        {
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Contenido de tarjeta"))
        },
        [nameof(MxChip)] = () => new()
        {
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Chip"))
        },
        [nameof(MxPaper)] = () => new()
        {
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Paper container"))
        },
        [nameof(MxTooltip)] = () => new()
        {
            ["Text"] = "Tooltip de ejemplo",
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Pasa el cursor aquí"))
        },
        [nameof(MxLink)] = () => new()
        {
            ["Href"] = "#",
            ["ChildContent"] = (RenderFragment)(b => b.AddContent(0, "Enlace de ejemplo"))
        },
        [nameof(MxTextField)] = () => new() { ["Label"] = "Nombre" },
        [nameof(MxCheckbox)] = () => new() { ["Label"] = "Acepto términos" },
        [nameof(MxSwitch)] = () => new() { ["Label"] = "Activar feature" },
        [nameof(MxSelect)] = () => new() { ["Label"] = "Selecciona una opción" },
        [nameof(MxDatePicker)] = () => new() { ["Label"] = "Fecha" },
        [nameof(MxTimePicker)] = () => new() { ["Label"] = "Hora" },
        [nameof(MxNumericField)] = () => new() { ["Label"] = "Cantidad" },
        [nameof(MxProgress)] = () => new() { ["Value"] = 55d },
    };

    public IReadOnlyList<ComponentShowcaseItem> Items { get; }

    public ComponentShowcaseRegistry()
    {
        Items = typeof(MxButton).Assembly
            .GetTypes()
            .Where(t => t is { IsPublic: true, IsAbstract: false } &&
                        t.Namespace == "Machsoft.DesignSystem.Components" &&
                        t.Name.StartsWith("Mx", StringComparison.Ordinal) &&
                        typeof(IComponent).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .Select(CreateItem)
            .ToArray();
    }

    public ComponentShowcaseItem? Find(string? key)
        => key is null ? null : Items.FirstOrDefault(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));

    private static ComponentShowcaseItem CreateItem(Type componentType)
    {
        var key = componentType.Name[2..].ToLowerInvariant();
        var sampleCode = BuildMarkup(componentType);
        return new ComponentShowcaseItem(
            Key: key,
            Name: componentType.Name,
            Type: componentType,
            SampleCode: sampleCode,
            Parameters: BuildParameters(componentType.Name));
    }

    private static string BuildMarkup(Type componentType)
    {
        var name = componentType.Name;
        return ParameterFactories.ContainsKey(name)
            ? $"<{name}>\n    <!-- ejemplo configurable -->\n</{name}>"
            : $"<{name} />";
    }

    private static IDictionary<string, object> BuildParameters(string componentName)
        => ParameterFactories.TryGetValue(componentName, out var factory)
            ? factory().Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value!)
            : new Dictionary<string, object>();
}

public sealed record ComponentShowcaseItem(
    string Key,
    string Name,
    Type Type,
    string SampleCode,
    IDictionary<string, object> Parameters);
