using MachSoft.DesignSystem.Theming;
using MudBlazor;

namespace MachSoft.UI.Theme;

public static class MxThemeFactory
{
    public static MudTheme Create(bool darkMode)
    {
        var t = darkMode ? ThemeCatalog.Dark : ThemeCatalog.Light;
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = ThemeCatalog.Light.Primary,
                Secondary = ThemeCatalog.Light.Secondary,
                Background = ThemeCatalog.Light.Background,
                Surface = ThemeCatalog.Light.Surface,
                Success = ThemeCatalog.Light.Success,
                Warning = ThemeCatalog.Light.Warning,
                Error = ThemeCatalog.Light.Error,
                TextPrimary = ThemeCatalog.Light.TextPrimary,
                TextSecondary = ThemeCatalog.Light.TextSecondary
            },
            PaletteDark = new PaletteDark
            {
                Primary = ThemeCatalog.Dark.Primary,
                Secondary = ThemeCatalog.Dark.Secondary,
                Background = ThemeCatalog.Dark.Background,
                Surface = ThemeCatalog.Dark.Surface,
                Success = ThemeCatalog.Dark.Success,
                Warning = ThemeCatalog.Dark.Warning,
                Error = ThemeCatalog.Dark.Error,
                TextPrimary = ThemeCatalog.Dark.TextPrimary,
                TextSecondary = ThemeCatalog.Dark.TextSecondary
            },
            Typography = new Typography
            {
                Default = new DefaultTypography { FontFamily = ["Inter", "Segoe UI", "sans-serif"], FontSize = "0.95rem" },
                H1 = new H1Typography { FontSize = "2rem", FontWeight = "700" },
                H2 = new H2Typography { FontSize = "1.5rem", FontWeight = "700" },
                H3 = new H3Typography { FontSize = "1.25rem", FontWeight = "600" }
            },
            ZIndex = new ZIndex { Dialog = t.ZIndexDialog }
        };
    }
}
