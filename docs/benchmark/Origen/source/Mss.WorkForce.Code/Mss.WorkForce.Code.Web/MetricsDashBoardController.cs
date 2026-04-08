using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using Mecalux.ITSW.ApplicationDictionary.Widgets;
using Microsoft.AspNetCore.DataProtection;
using Mss.WorkForce.Code.Models.DataAccess;
using System.Xml.Linq;

namespace Mss.WorkForce.Code.Web
{
    public class MetricsDashBoardController : DashboardController
    {
        public MetricsDashBoardController(DashboardConfigurator configurator, IDataProtectionProvider? dataProtectionProvider = null) : base(configurator, dataProtectionProvider)
        {
            DashboardInMemoryStorage dashboardStorage = new DashboardInMemoryStorage();

            var widgets = DataAccess.GetMetrics();
            IWidgetPanelManager panelManager = WidgetPanelManagerFactory.Create();
            IWidgetPanelManager panelManagerNew = WidgetPanelManagerFactory.Create();

            var settings = new WidgetPanelDecomposeSettings();
            settings.WidgetNames = DataAccess.GetMetricsTitles();
            List<WidgetDefinition> allwidgets = new List<WidgetDefinition>();

            //Por el momento se comenta para pruebas
            //foreach (var widget in widgets)
            //{
            //    var pdef = panelManager.Decompose(widget, settings);
            //    allwidgets.AddRange(pdef.Widgets);
            //}

            //var allpanel = panelManagerNew.Compose(new WidgetPanelDefinition("",
            // allwidgets.Select(x => new WidgetDefinition(x.Name, x.Code, x.CalculatedFields,
            //x.Filter, x.Title, true, x.Parameters))));

            widgets[0] = widgets[0].Replace("FromAppConfig=\"False\"", "FromAppConfig =\"True\"");
            string dashboardId = $"DynamicDashboard1";
            XDocument dashboardXml = XDocument.Parse(widgets[0]);
            dashboardStorage.RegisterDashboard(dashboardId, dashboardXml);
            configurator.SetDashboardStorage(dashboardStorage);
        }
    }
}
