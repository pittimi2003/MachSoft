namespace MachSoft.UI.Showcase.Models;

public static class CatalogRegistry
{
    public static IReadOnlyList<(string Title, string Route)> FoundationPages { get; } =
    [
        ("Colors", "/foundations/colors")
    ];

    public static IReadOnlyList<(string Title, string Route)> ComponentPages { get; } =
    [
        ("Buttons", "/components/buttons"),
        ("TextFields", "/components/textfields"),
        ("Alerts", "/components/alerts"),
        ("Table", "/components/table")
    ];
}
