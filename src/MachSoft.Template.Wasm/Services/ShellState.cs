namespace MachSoft.Template.Wasm.Services;

public sealed class ShellState
{
    public bool DarkMode { get; private set; }

    public event Action? Changed;

    public void SetDarkMode(bool darkMode)
    {
        if (DarkMode == darkMode)
        {
            return;
        }

        DarkMode = darkMode;
        Changed?.Invoke();
    }
}
