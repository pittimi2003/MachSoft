namespace MachSoft.DesignSystem.Tokens;

public static class ColorTokens
{
    public static class Brand
    {
        public const string Accent = "#00B5D8";
        public const string AccentHover = "#0099BA";
        public const string AccentActive = "#007F99";
        public const string AccentSoft = "#D9F5FB";

        public const string DeepBlue = "#123E73";
        public const string DeepBlueHover = "#103760";
        public const string DeepBlueActive = "#0D2E50";
        public const string DeepBlueSoft = "#E5ECF5";
    }

    public static class Neutral
    {
        public const string White = "#FFFFFF";
        public const string Graphite050 = "#F3F6F8";
        public const string Graphite100 = "#E7EDF2";
        public const string Graphite200 = "#D4DEE6";
        public const string Graphite300 = "#B9C7D3";
        public const string Graphite400 = "#94A6B6";
        public const string Graphite500 = "#708497";
        public const string Graphite600 = "#556779";
        public const string Graphite700 = "#3E4E5E";
        public const string Graphite800 = "#2B3947";
        public const string Graphite900 = "#1A2530";
        public const string Black = "#111A22";
    }

    public static class Semantic
    {
        public const string Info = "#1870D5";
        public const string Success = "#1F8A54";
        public const string Warning = "#B97810";
        public const string Error = "#C23844";
    }

    public static class Surface
    {
        public const string LightBackground = Neutral.Graphite050;
        public const string LightShell = Neutral.Graphite100;
        public const string LightSurface = Neutral.White;
        public const string LightSurfaceAlt = Neutral.Graphite100;
        public const string LightSurfaceRaised = "#FCFDFE";
        public const string LightBorder = Neutral.Graphite200;
        public const string LightBorderStrong = Neutral.Graphite300;

        public const string DarkBackground = "#101922";
        public const string DarkShell = "#152231";
        public const string DarkSurface = "#1A2A3A";
        public const string DarkSurfaceAlt = "#223548";
        public const string DarkSurfaceRaised = "#293E53";
        public const string DarkBorder = "#344C62";
        public const string DarkBorderStrong = "#44607A";
    }

    public static class Text
    {
        public const string LightPrimary = Neutral.Graphite900;
        public const string LightSecondary = Neutral.Graphite700;
        public const string LightMuted = Neutral.Graphite500;
        public const string LightInverse = Neutral.White;

        public const string DarkPrimary = "#F4F8FB";
        public const string DarkSecondary = "#BED0DE";
        public const string DarkMuted = "#8FA5B7";
        public const string DarkInverse = Neutral.Graphite900;
    }

    public static class Focus
    {
        public const string Ring = Brand.Accent;
        public const string RingContrast = "#7EE2F4";
    }
}
