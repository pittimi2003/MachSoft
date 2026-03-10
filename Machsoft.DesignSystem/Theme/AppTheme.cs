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
            OverlayDark = AppColors.Overlay
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = AppLayout.RadiusMd
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["Roboto", "Helvetica Neue", "Arial", "sans-serif"],
                FontSize = AppTypography.FontSizeMd,
                FontWeight = AppTypography.BodyWeight
            },
            H1 = new H1
            {
                FontFamily = ["Roboto", "Helvetica Neue", "Arial", "sans-serif"],
                FontWeight = AppTypography.HeadingWeight
            }
        }
    };
}
