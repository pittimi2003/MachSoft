namespace MachSoft.UI.Showcase.Models;

public sealed class ShowcaseThemeState
{
    public bool IsDarkMode { get; private set; }

    public event Action? Changed;

    public void Set(bool darkMode)
    {
        if (IsDarkMode == darkMode)
        {
            return;
        }

        IsDarkMode = darkMode;
        Changed?.Invoke();
    }
}
