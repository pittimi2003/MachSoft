namespace MachSoft.Template.Server.Services;

public sealed class ShellState
{
    public bool DarkMode { get; private set; }

    public bool WorkSelectionActive { get; private set; }

    public int WorkSelectionCount { get; private set; }

    public string? WorkSelectionTitle { get; private set; }

    public IReadOnlyList<string> WorkSelectionDetails { get; private set; } = Array.Empty<string>();

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

    public void SetWorkSelection(int selectedCount, string? title, IReadOnlyList<string>? details)
    {
        var normalizedCount = Math.Max(0, selectedCount);
        var normalizedActive = normalizedCount > 0;
        var normalizedDetails = details ?? Array.Empty<string>();
        var normalizedTitle = normalizedActive ? title : null;

        WorkSelectionActive = normalizedActive;
        WorkSelectionCount = normalizedCount;
        WorkSelectionTitle = normalizedTitle;
        WorkSelectionDetails = normalizedDetails;
        Changed?.Invoke();
    }

    public void ClearWorkSelection()
    {
        SetWorkSelection(0, null, Array.Empty<string>());
    }
}
