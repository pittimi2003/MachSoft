using Microsoft.JSInterop;

namespace MachSoft.UI.Showcase.Theme;

public sealed class ShowcaseThemeState
{
    private const string ThemeStorageKey = "mx-showcase-theme";

    public event Action? OnChange;

    public bool DarkMode { get; private set; }

    public async Task InitializeAsync(IJSRuntime jsRuntime)
    {
        var theme = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ThemeStorageKey);
        DarkMode = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase);
        OnChange?.Invoke();
    }

    public async Task ToggleAsync(IJSRuntime jsRuntime)
    {
        DarkMode = !DarkMode;
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemeStorageKey, DarkMode ? "dark" : "light");
        OnChange?.Invoke();
    }
}
