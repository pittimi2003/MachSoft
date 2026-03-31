using Microsoft.JSInterop;

namespace MachSoft.UI.Showcase.Theme;

public sealed class ShowcaseThemeState
{
    private const string ThemeStorageKey = "mx-showcase-theme";

    public event Action? OnChange;

    public bool DarkMode { get; private set; }

    public bool IsInitialized { get; private set; }

    public async Task InitializeAsync(IJSRuntime js)
    {
        if (IsInitialized)
        {
            return;
        }

        var storedTheme = await js.InvokeAsync<string?>("machsoftTheme.get", ThemeStorageKey);
        var isDark = string.Equals(storedTheme, "dark", StringComparison.OrdinalIgnoreCase);

        DarkMode = isDark;
        IsInitialized = true;
        OnChange?.Invoke();

        if (!string.Equals(storedTheme, "light", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(storedTheme, "dark", StringComparison.OrdinalIgnoreCase))
        {
            await PersistAsync(js);
        }
    }

    public Task ToggleAsync(IJSRuntime js) => SetDarkModeAsync(!DarkMode, js);

    public async Task SetDarkModeAsync(bool darkMode, IJSRuntime js)
    {
        if (DarkMode == darkMode && IsInitialized)
        {
            return;
        }

        DarkMode = darkMode;
        IsInitialized = true;
        OnChange?.Invoke();
        await PersistAsync(js);
    }

    private Task PersistAsync(IJSRuntime js)
    {
        var theme = DarkMode ? "dark" : "light";
        return js.InvokeVoidAsync("machsoftTheme.set", ThemeStorageKey, theme).AsTask();
    }
}
