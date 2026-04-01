using Microsoft.AspNetCore.Components;

namespace MachSoft.UI.Models;

public sealed class MxDataGridColumn<TItem>
{
    public MxDataGridColumn(string key, string header, Func<TItem, object?> valueSelector)
    {
        Key = key;
        Header = header;
        ValueSelector = valueSelector;
    }

    public string Key { get; }
    public string Header { get; }
    public Func<TItem, object?> ValueSelector { get; }
    public string? Width { get; init; }
    public string Align { get; init; } = "left";
    public bool IsPrimary { get; init; }
    public RenderFragment<TItem>? CellTemplate { get; init; }
}
