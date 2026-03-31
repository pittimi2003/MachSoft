using MachSoft.DesignSystem.Tokens;

namespace MachSoft.DesignSystem.Theming;

public static class ThemeCatalog
{
    public static ThemeContract Light { get; } = new(
        ColorTokens.Neutral.Gray50,
        ColorTokens.Neutral.White,
        ColorTokens.Neutral.Black,
        ColorTokens.Neutral.Gray700,
        ColorTokens.Brand.Primary,
        ColorTokens.Brand.Secondary,
        ColorTokens.Semantic.Success,
        ColorTokens.Semantic.Warning,
        ColorTokens.Semantic.Error,
        ColorTokens.Brand.Accent,
        1400,
        "comfortable");

    public static ThemeContract Dark { get; } = new(
        "#0B1220",
        "#131D2E",
        ColorTokens.Neutral.White,
        "#D4D9E1",
        "#5DA7F2",
        "#4FD1BA",
        "#34D399",
        "#FBBF24",
        "#F87171",
        "#F29E4C",
        1400,
        "comfortable");
}
