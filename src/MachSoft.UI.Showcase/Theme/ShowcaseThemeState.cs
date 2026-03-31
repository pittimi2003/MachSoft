namespace MachSoft.UI.Showcase.Theme;

public sealed class ShowcaseThemeState
{
    public event Action? OnChange;

    public bool DarkMode { get; private set; }

    public void Toggle() => SetDarkMode(!DarkMode);

    public void SetDarkMode(bool darkMode)
    {
        if (DarkMode == darkMode)
        {
            return;
        }

        DarkMode = darkMode;
        OnChange?.Invoke();
    }
}
