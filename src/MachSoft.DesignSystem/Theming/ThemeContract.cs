namespace MachSoft.DesignSystem.Theming;

public sealed record ThemeContract(
    string Background,
    string Surface,
    string TextPrimary,
    string TextSecondary,
    string Primary,
    string Secondary,
    string Success,
    string Warning,
    string Error,
    string FocusRing,
    int ZIndexDialog,
    string Density);
