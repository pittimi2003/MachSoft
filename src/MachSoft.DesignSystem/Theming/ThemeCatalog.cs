using MachSoft.DesignSystem.Tokens;

namespace MachSoft.DesignSystem.Theming;

public static class ThemeCatalog
{
    public static ThemeContract Light { get; } = new(
        ColorTokens.Surface.LightBackground,
        ColorTokens.Surface.LightShell,
        ColorTokens.Surface.LightSurface,
        ColorTokens.Surface.LightSurfaceAlt,
        ColorTokens.Surface.LightSurfaceRaised,
        ColorTokens.Surface.LightBorder,
        ColorTokens.Surface.LightBorderStrong,
        ColorTokens.Text.LightPrimary,
        ColorTokens.Text.LightSecondary,
        ColorTokens.Text.LightMuted,
        ColorTokens.Text.LightInverse,
        ColorTokens.Brand.Accent,
        ColorTokens.Brand.AccentHover,
        ColorTokens.Brand.AccentActive,
        ColorTokens.Brand.AccentSoft,
        ColorTokens.Brand.DeepBlue,
        ColorTokens.Brand.DeepBlueSoft,
        ColorTokens.Brand.Accent,
        ColorTokens.Semantic.Success,
        ColorTokens.Semantic.Warning,
        ColorTokens.Semantic.Error,
        ColorTokens.Semantic.Info,
        ColorTokens.Focus.Ring,
        ColorTokens.Focus.RingContrast,
        1400,
        "compact");

    public static ThemeContract Dark { get; } = new(
        ColorTokens.Surface.DarkBackground,
        ColorTokens.Surface.DarkShell,
        ColorTokens.Surface.DarkSurface,
        ColorTokens.Surface.DarkSurfaceAlt,
        ColorTokens.Surface.DarkSurfaceRaised,
        ColorTokens.Surface.DarkBorder,
        ColorTokens.Surface.DarkBorderStrong,
        ColorTokens.Text.DarkPrimary,
        ColorTokens.Text.DarkSecondary,
        ColorTokens.Text.DarkMuted,
        ColorTokens.Text.DarkInverse,
        "#22C7E6",
        "#18B3D1",
        "#1094B0",
        "#123745",
        "#5F9AD8",
        "#1A2F45",
        "#66D9F0",
        "#48B579",
        "#D8993A",
        "#E16A75",
        "#63B0F4",
        "#66D9F0",
        "#9BE9F8",
        1400,
        "compact");
}
