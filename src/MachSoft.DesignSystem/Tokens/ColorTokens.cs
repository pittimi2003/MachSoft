namespace MachSoft.DesignSystem.Tokens;

public static class ColorTokens
{
    public static class Brand
    {
        public const string Primary = "#005098";
        public const string PrimaryHover = "#00457F";
        public const string PrimaryActive = "#003A6A";
        public const string PrimarySoft = "#E6EEF6";
        public const string TechnicalAccent = "#00AFCF";
        public const string TechnicalAccentSoft = "#E3F8FC";
    }

    public static class Neutral
    {
        public const string White = "#FFFFFF";
        public const string Slate050 = "#F4F7FB";
        public const string Slate100 = "#E9EEF5";
        public const string Slate200 = "#D8E0EA";
        public const string Slate300 = "#C0CBD9";
        public const string Slate400 = "#96A5B8";
        public const string Slate500 = "#73849A";
        public const string Slate600 = "#55667B";
        public const string Slate700 = "#3D4B5D";
        public const string Slate800 = "#243142";
        public const string Slate900 = "#121B28";
        public const string Black = "#0C1118";
    }

    public static class Semantic
    {
        public const string Info = "#0068C9";
        public const string Success = "#1B7A48";
        public const string Warning = "#B06A00";
        public const string Error = "#B4232D";
    }

    public static class Surface
    {
        public const string LightBackground = Neutral.Slate050;
        public const string LightSurface = Neutral.White;
        public const string LightSurfaceAlt = Neutral.Slate100;
        public const string LightBorder = Neutral.Slate200;

        public const string DarkBackground = "#0D1522";
        public const string DarkSurface = "#131F31";
        public const string DarkSurfaceAlt = "#1A2940";
        public const string DarkBorder = "#2C3D56";
    }

    public static class Text
    {
        public const string LightPrimary = Neutral.Slate900;
        public const string LightSecondary = Neutral.Slate600;
        public const string LightMuted = Neutral.Slate500;

        public const string DarkPrimary = Neutral.White;
        public const string DarkSecondary = "#B4C0D0";
        public const string DarkMuted = "#8EA0B8";
    }

    public static class Focus
    {
        public const string Ring = Brand.Primary;
        public const string RingContrast = Brand.TechnicalAccent;
    }
}
