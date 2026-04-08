using Microsoft.AspNetCore.DataProtection;
using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardWeb;
using System.Xml.Linq;
using Mecalux.ITSW.ApplicationDictionary.Widgets;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Web.Code;
using Mss.WorkForce.Code.Web.Services;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web
{
    public class DefaultDashboardController : DashboardController
    {
        private readonly GlobalSettings _globalSettings;
        private readonly ILocalizationService _localizationService;
        private readonly DataAccess DataAccess;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Dictionary<(eGroupWidgets GroupWidgets, string Culture), XDocument> CachedBaseDashboards = new();

        public DefaultDashboardController(
            DashboardConfigurator configurator,
            GlobalSettings globalSettings,
            ILocalizationService localizationService,
            DataAccess dataAccess,
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider? dataProtectionProvider = null)
            : base(configurator, dataProtectionProvider)
        {
            DataAccess = dataAccess;
            _globalSettings = globalSettings;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;

            var dashboardStorage = new DashboardInMemoryStorage();
            var settings = new WidgetPanelDecomposeSettings();

            eGroupWidgets selectedGroup = _globalSettings.GroupWidgets;
            Guid currentPlanning = _globalSettings.CurrentPlanning;

            var ctx = _httpContextAccessor.HttpContext;
           
            if (ctx != null) {
                if (ctx.Request.Headers.TryGetValue("GroupWidgets", out var headerGroup))
                {
                    if (Enum.TryParse(typeof(eGroupWidgets), headerGroup!, true, out var parsed))
                        selectedGroup = (eGroupWidgets)parsed;
                }

                if (ctx.Request.Headers.TryGetValue("CurrentPlanning", out var headerCurrentPlanning) &&
                    Guid.TryParse(headerCurrentPlanning, out var parsedPlanning))
                    currentPlanning = parsedPlanning;
            }

            var cacheKey = (selectedGroup, Thread.CurrentThread.CurrentUICulture.Name);

            // Recuperamos XML base o lo generamos por primera vez
            if (!CachedBaseDashboards.TryGetValue(cacheKey, out var baseXml))
            {
                List<string> widgets = new();
                switch (selectedGroup)
                {
                    case eGroupWidgets.Planning:
                        widgets = DataAccess.GetWidgets();
                        settings.WidgetNames = DataAccess.GetWidgetsTitles();
                        break;

                    case eGroupWidgets.Workers:
                        widgets = DataAccess.GetWidgetsWorkers();
                        settings.WidgetNames = DataAccess.GetWidgetsTitlesworkers();
                        break;
                }

                widgets[0] = widgets[0].Replace("FromAppConfig=\"False\"", "FromAppConfig=\"True\"");

                var xml = XDocument.Parse(widgets[0]);

                xml = DeleteColorScheme(xml);
                _localizationService.Loc(xml);

                CachedBaseDashboards[cacheKey] = new XDocument(xml);
                baseXml = CachedBaseDashboards[cacheKey];
            }

            XDocument dashboardXml = new XDocument(baseXml);

            bool isHorizontal = !_globalSettings.widgetOrientation;
            var layoutGroups = dashboardXml.Descendants("LayoutGroup").Skip(1);
            foreach (var layoutGroup in layoutGroups)
            {
                string newOrientation = isHorizontal ? "Horizontal" : "Vertical";
                layoutGroup.SetAttributeValue("Orientation", newOrientation);
            }

            string planningId = currentPlanning.ToString();
            string utcWarehouse = $"{_globalSettings.UtcWarehouse} hour";
            string hourFormat = _globalSettings.HourFormat;

            Guid warehouseId = DataAccess.GetWarehouseIdByPlanningId(currentPlanning);

            dashboardXml = DeleteColorScheme(dashboardXml);

            InjectArguments(dashboardXml, currentPlanning.ToString(),utcWarehouse, globalSettings.HourFormat, warehouseId.ToString());
            if (selectedGroup == eGroupWidgets.Workers)
                AddPlanningFilterToQueries(dashboardXml);

            string dashboardId = "DynamicDashboard1";
            dashboardStorage.RegisterDashboard(dashboardId, dashboardXml);
            configurator.SetDashboardStorage(dashboardStorage);

            configurator.AllowExecutingCustomSql = true;

            DashboardUtils.CreateDashboardConfigurator(configurator);
        }

        private void Configurator_CustomParameters(object sender, CustomParametersWebEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void InjectArguments(XDocument dashboardXml, string planningId, string utcOffset, string hourFormat, string warehouseId)
        {
            foreach (var sqlNode in dashboardXml.Descendants("Sql"))
            {
                    var v = sqlNode.Value;
                    if (v.Contains("{PlanningId}")) v = v.Replace("{PlanningId}", planningId);
                    if (v.Contains("{UtcOffset}")) v = v.Replace("{UtcOffset}", utcOffset);
                    if (v.Contains("{HourFormat}")) v = v.Replace("{HourFormat}", MapHourFormatToPg(hourFormat));
                    if (v.Contains("{WarehouseId}")) v = v.Replace("{WarehouseId}", warehouseId);
                sqlNode.Value = v;
            }
        }

        private void AddPlanningFilterToQueries(XDocument dashboardXml)
        {
            string planningId = _globalSettings.CurrentPlanning.ToString();

            XElement EnsureParameters()
            {
                var p = dashboardXml.Root.Element("Parameters");
                if (p == null)
                {
                    p = new XElement("Parameters");
                    dashboardXml.Root.AddFirst(p);
                }
                return p;
            }

            // Nombres de query a los que SÍ se aplica el filtro
            var targetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "WFMLaborWorkerPerProcessType",
                "WFMLaborWorker"
            };

            var targetQueries = dashboardXml
                    .Descendants("Query")
                    .Where(q => targetNames.Contains((string)q.Attribute("Name")))
                    .ToList();

            if (targetQueries.Count == 0)
                return;

            string filter = $"[PlanningId] = '{planningId}'";

            foreach (var query in targetQueries)
            {
                var filterNode = query.Element("FilterString");
                if (filterNode == null)
                {
                    query.Add(new XElement("FilterString", filter));
                }
                else
                {
                    filterNode.Value = filter;
                }
            }
        }

        private XDocument DeleteColorScheme(XDocument document)
        {
            if (document.Element("Dashboard") != null && document.Element("Dashboard").Element("Items") != null)
            {
                //The ColorScheme node is removed from items that have it
                document.Element("Dashboard").Element("Items").Descendants().ToList().ForEach(w =>
                {
                    if (w.Element("ColorScheme") != null)
                        w.Element("ColorScheme").Remove();
                });
            }
            return document;
        }

        private static string MapHourFormatToPg(string hourFormat)
        {
            switch (hourFormat)
            {
                case "hh:mm:ss tt": return "HH12:MI:SS AM";
                case "hh:mm tt": return "HH12:MI AM";
                case "HH:mm:ss": return "HH24:MI:SS";
                case "HH:mm": return "HH24:MI";
                case "HH.mm.ss": return "HH24.MI.SS";
                case "HH.mm": return "HH24.MI";
                default: return "HH24:MI";
            }
        }


    }
}
