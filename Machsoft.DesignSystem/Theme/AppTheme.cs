using MudBlazor;

namespace Machsoft.DesignSystem.Theme;

public static class AppTheme
{
    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = AppColors.Primary,
            Secondary = AppColors.Secondary,
            Background = AppColors.Background,
            Surface = AppColors.Surface,
            AppbarBackground = AppColors.Surface,
            AppbarText = AppColors.TextPrimary,
            DrawerBackground = AppColors.Surface,
            DrawerText = AppColors.TextPrimary,
            DrawerIcon = AppColors.Primary,
            TextPrimary = AppColors.TextPrimary,
            TextSecondary = AppColors.TextSecondary,
            LinesDefault = AppColors.Border,
            OverlayDark = AppColors.Overlay,
            Success = AppColors.Success,
            Warning = AppColors.Warning,
            Error = AppColors.Error,
            Info = AppColors.Info
        },
        PaletteDark = new PaletteDark
        {
            Primary = AppColors.Dark.Primary,
            Secondary = AppColors.Dark.Secondary,
            Background = AppColors.Dark.Background,
            Surface = AppColors.Dark.Surface,
            AppbarBackground = AppColors.Dark.Surface,
            AppbarText = AppColors.Dark.TextPrimary,
            DrawerBackground = AppColors.Dark.Surface,
            DrawerText = AppColors.Dark.TextPrimary,
            DrawerIcon = AppColors.Dark.Primary,
            TextPrimary = AppColors.Dark.TextPrimary,
            TextSecondary = AppColors.Dark.TextSecondary,
            LinesDefault = AppColors.Dark.Border,
            OverlayDark = AppColors.Dark.Overlay,
            Success = AppColors.Dark.Success,
            Warning = AppColors.Dark.Warning,
            Error = AppColors.Dark.Error,
            Info = AppColors.Dark.Info
        },
        PaletteDark = new PaletteDark
        {
            Primary = AppColors.Dark.Primary,
            Secondary = AppColors.Dark.Secondary,
            Background = AppColors.Dark.Background,
            Surface = AppColors.Dark.Surface,
            AppbarBackground = AppColors.Dark.Surface,
            AppbarText = AppColors.Dark.TextPrimary,
            DrawerBackground = AppColors.Dark.Surface,
            DrawerText = AppColors.Dark.TextPrimary,
            DrawerIcon = AppColors.Dark.Primary,
            TextPrimary = AppColors.Dark.TextPrimary,
            TextSecondary = AppColors.Dark.TextSecondary,
            LinesDefault = AppColors.Dark.Border,
            OverlayDark = AppColors.Dark.Overlay
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = AppLayout.RadiusMd
        },
        Typography = new Typography
        {
            Default = BuildDefault(AppTypography.FontSizeMd),
            H1 = BuildH1(AppTypography.FontSize4xl),
            H2 = BuildH2(AppTypography.FontSize3xl),
            H3 = BuildH3(AppTypography.FontSize2xl),
            H4 = BuildH4(AppTypography.FontSizeXl),
            H5 = BuildH5(AppTypography.FontSizeLg),
            H6 = BuildH6(AppTypography.FontSizeMd),
            Body1 = BuildBody1(AppTypography.FontSizeMd),
            Body2 = BuildBody2(AppTypography.FontSizeSm),
            Caption = BuildCaption(AppTypography.FontSizeXs),
            Button = BuildButton(),
            Subtitle1 = BuildSubtitle1(AppTypography.FontSizeLg),
            Subtitle2 = BuildSubtitle2(AppTypography.FontSizeMd),
            Overline = BuildOverline()
        }
    };

    private static string[] FontFamily => ["Roboto", "Helvetica Neue", "Arial", "sans-serif"];

    private static Default BuildDefault(string fontSize) => new()
    {
        FontFamily = FontFamily,
        FontSize = fontSize,
        FontWeight = AppTypography.BodyWeight,
        LineHeight = AppTypography.LineHeightNormal,
    };

    private static H1 BuildH1(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.HeadingWeight, LineHeight = AppTypography.LineHeightTight, LetterSpacing = AppTypography.LetterSpacingTight };
    private static H2 BuildH2(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.HeadingWeight, LineHeight = AppTypography.LineHeightTight, LetterSpacing = AppTypography.LetterSpacingTight };
    private static H3 BuildH3(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.HeadingWeight, LineHeight = AppTypography.LineHeightTight, LetterSpacing = AppTypography.LetterSpacingTight };
    private static H4 BuildH4(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.HeadingWeight, LineHeight = AppTypography.LineHeightTight, LetterSpacing = AppTypography.LetterSpacingTight };
    private static H5 BuildH5(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.HeadingWeight, LineHeight = AppTypography.LineHeightTight, LetterSpacing = AppTypography.LetterSpacingNormal };
    private static H6 BuildH6(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.HeadingWeight, LineHeight = AppTypography.LineHeightNormal, LetterSpacing = AppTypography.LetterSpacingNormal };

    private static Body1 BuildBody1(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.BodyWeight, LineHeight = AppTypography.LineHeightNormal, LetterSpacing = AppTypography.LetterSpacingNormal };
    private static Body2 BuildBody2(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.BodyWeight, LineHeight = AppTypography.LineHeightNormal, LetterSpacing = AppTypography.LetterSpacingNormal };
    private static Caption BuildCaption(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = AppTypography.BodyWeight, LineHeight = AppTypography.LineHeightNormal, LetterSpacing = AppTypography.LetterSpacingNormal };
    private static Subtitle1 BuildSubtitle1(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = "500", LineHeight = AppTypography.LineHeightNormal, LetterSpacing = AppTypography.LetterSpacingNormal };
    private static Subtitle2 BuildSubtitle2(string fontSize) => new() { FontFamily = FontFamily, FontSize = fontSize, FontWeight = "500", LineHeight = AppTypography.LineHeightNormal, LetterSpacing = AppTypography.LetterSpacingNormal };

    private static Button BuildButton() => new()
    {
        FontFamily = FontFamily,
        FontSize = AppTypography.FontSizeSm,
        FontWeight = "500",
        LineHeight = AppTypography.LineHeightNormal,
        LetterSpacing = AppTypography.LetterSpacingWide,
        TextTransform = "none"
    };

    private static Overline BuildOverline() => new()
    {
        FontFamily = FontFamily,
        FontSize = AppTypography.FontSizeXs,
        FontWeight = "500",
        LineHeight = AppTypography.LineHeightNormal,
        LetterSpacing = AppTypography.LetterSpacingWide,
        TextTransform = "uppercase"
    };
}
