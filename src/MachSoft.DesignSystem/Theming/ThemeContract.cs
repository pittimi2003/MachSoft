namespace MachSoft.DesignSystem.Theming;

public sealed record ThemeContract(
    string Background,
    string Surface,
    string SurfaceAlt,
    string Border,
    string TextPrimary,
    string TextSecondary,
    string TextMuted,
    string Primary,
    string PrimaryHover,
    string PrimaryActive,
    string TechnicalAccent,
    string Link,
    string Success,
    string Warning,
    string Error,
    string Info,
    string FocusRing,
    string FocusRingContrast,
    int ZIndexDialog,
    string Density);
