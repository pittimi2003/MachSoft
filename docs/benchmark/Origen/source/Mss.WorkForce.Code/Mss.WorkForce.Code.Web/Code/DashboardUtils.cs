using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using System.Drawing;

namespace Mss.WorkForce.Code.Web.Code
{
    public static class DashboardUtils
    {
        public static void CreateDashboardConfigurator(DashboardConfigurator configurator)
        {
            configurator.CustomPalette += Default_CustomPalette;
        }

        static void Default_CustomPalette(object sender, CustomPaletteWebEventArgs e)
        {
            // Create a new custom palette.
            List<Color> customColors = new List<Color>();
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.BlueLight600));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Red400));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Smoke300));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Yellow600));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Purple500));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Turquese500));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Pink300));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Blue400));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.BlueLight400));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Red300));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Smoke500));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Yellow400));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Purple300));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Turquese700));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Pink500));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Blue300));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.BlueLight200));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Red100));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Smoke200));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Yellow100));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Purple100));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Turquese100));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Pink200));
            customColors.Add(ColorTranslator.FromHtml(DashboardColor.Blue100));

            // Assign a newly created custom palette to the Web Dashboard.
            e.Palette = new DashboardPalette(customColors);
        }

    }
}
