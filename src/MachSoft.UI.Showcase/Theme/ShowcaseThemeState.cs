namespace MachSoft.UI.Showcase.Theme;

public sealed class ShowcaseThemeState
{
    public bool DarkMode { get; private set; }

    public void Toggle() => DarkMode = !DarkMode;
}
