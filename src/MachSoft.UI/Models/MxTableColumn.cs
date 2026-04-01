namespace MachSoft.UI.Models;

public sealed record MxTableColumn<TItem>(
    string Key,
    string Header,
    Func<TItem, object?> ValueSelector,
    bool IsPrimary = false,
    string? Width = null,
    string? Align = null);
