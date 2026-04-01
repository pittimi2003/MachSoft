using Microsoft.AspNetCore.Components;

namespace MachSoft.UI.Models;

public sealed record MxTabItem(string Key, string Label, RenderFragment? Content = null, string? Icon = null, bool Disabled = false);
