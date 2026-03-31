using MachSoft.DesignSystem.Tokens;

namespace MachSoft.DesignSystem.Theming;

public static class ThemeCatalog
{
    public static ThemeContract Light { get; } = new(
        ColorTokens.Surface.LightBackground,
        ColorTokens.Surface.LightSurface,
        ColorTokens.Surface.LightSurfaceAlt,
        ColorTokens.Surface.LightBorder,
        ColorTokens.Text.LightPrimary,
        ColorTokens.Text.LightSecondary,
        ColorTokens.Text.LightMuted,
        ColorTokens.Brand.Primary,
        ColorTokens.Brand.PrimaryHover,
        ColorTokens.Brand.PrimaryActive,
        ColorTokens.Brand.TechnicalAccent,
        ColorTokens.Brand.Primary,
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
        ColorTokens.Surface.DarkSurface,
        ColorTokens.Surface.DarkSurfaceAlt,
        ColorTokens.Surface.DarkBorder,
        ColorTokens.Text.DarkPrimary,
        ColorTokens.Text.DarkSecondary,
        ColorTokens.Text.DarkMuted,
        "#2E78BC",
        "#266AA7",
        "#1E588D",
        "#34CAE7",
        "#7AB8F0",
        "#38A169",
        "#D28A20",
        "#DB5B63",
        "#53A6F0",
        "#7AB8F0",
        "#34CAE7",
        1400,
        "compact");
}
