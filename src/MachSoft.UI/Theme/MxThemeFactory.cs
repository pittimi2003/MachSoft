using MachSoft.DesignSystem.Theming;
using MudBlazor;

namespace MachSoft.UI.Theme;

public static class MxThemeFactory
{
    public static MudTheme Create(bool darkMode)
    {
        var light = ThemeCatalog.Light;
        var dark = ThemeCatalog.Dark;

        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = light.Accent,
                PrimaryDarken = light.AccentHover,
                PrimaryLighten = light.Accent,
                Secondary = light.DeepBlue,
                Tertiary = light.DeepBlue,
                Info = light.Info,
                Success = light.Success,
                Warning = light.Warning,
                Error = light.Error,
                Background = light.Background,
                Surface = light.Surface,
                AppbarBackground = light.Shell,
                DrawerBackground = light.Shell,
                DrawerText = light.TextPrimary,
                DrawerIcon = light.TextSecondary,
                TextPrimary = light.TextPrimary,
                TextSecondary = light.TextSecondary,
                ActionDefault = light.TextSecondary,
                ActionDisabled = light.TextMuted,
                ActionDisabledBackground = light.SurfaceAlt,
                Divider = light.Border,
                LinesDefault = light.Border,
                TableLines = light.Border,
                AppbarText = light.TextPrimary
            },
            PaletteDark = new PaletteDark
            {
                Primary = dark.Accent,
                PrimaryDarken = dark.AccentHover,
                PrimaryLighten = dark.Accent,
                Secondary = dark.DeepBlue,
                Tertiary = dark.DeepBlue,
                Info = dark.Info,
                Success = dark.Success,
                Warning = dark.Warning,
                Error = dark.Error,
                Background = dark.Background,
                Surface = dark.Surface,
                AppbarBackground = dark.Shell,
                DrawerBackground = dark.Shell,
                DrawerText = dark.TextPrimary,
                DrawerIcon = dark.TextSecondary,
                TextPrimary = dark.TextPrimary,
                TextSecondary = dark.TextSecondary,
                ActionDefault = dark.TextSecondary,
                ActionDisabled = dark.TextMuted,
                ActionDisabledBackground = dark.SurfaceAlt,
                Divider = dark.Border,
                LinesDefault = dark.Border,
                TableLines = dark.Border,
                AppbarText = dark.TextPrimary
            },
            LayoutProperties = new LayoutProperties { DefaultBorderRadius = "4px" },
            Typography = new Typography
            {
                Default = new Default { FontFamily = ["Inter", "Segoe UI", "sans-serif"], FontSize = "0.875rem", LineHeight = 1.5 },
                H1 = new H1 { FontSize = "2.25rem", FontWeight = 700, LineHeight = 1.2, LetterSpacing = "-0.01em" },
                H2 = new H2 { FontSize = "1.875rem", FontWeight = 700, LineHeight = 1.25 },
                H3 = new H3 { FontSize = "1.5rem", FontWeight = 600, LineHeight = 1.3 },
                H4 = new H4 { FontSize = "1.25rem", FontWeight = 600, LineHeight = 1.35 },
                H5 = new H5 { FontSize = "1.125rem", FontWeight = 600 },
                H6 = new H6 { FontSize = "1rem", FontWeight = 600 },
                Subtitle1 = new Subtitle1 { FontSize = "0.95rem", FontWeight = 600 },
                Subtitle2 = new Subtitle2 { FontSize = "0.875rem", FontWeight = 600 },
                Body1 = new Body1 { FontSize = "0.95rem", LineHeight = 1.55 },
                Body2 = new Body2 { FontSize = "0.875rem", LineHeight = 1.5 },
                Button = new Button { FontSize = "0.8125rem", FontWeight = 600, TextTransform = "none", LetterSpacing = "0.01em" }
            },
            ZIndex = new ZIndex { Dialog = darkMode ? dark.ZIndexDialog : light.ZIndexDialog }
        };
    }
}
