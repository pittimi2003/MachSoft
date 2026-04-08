using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Components.Pages.Planning
{
    public partial class LiveSchedulerGrid: GanttOperations, IDisposable
    {
        public List<VehicleMetricsDto> VehicleMetrics { get; set; } = new List<VehicleMetricsDto>();

        DxGrid gridLoad {  get; set; }

        DateTime DayStart = DateTime.Today;
        DateTime DayEnd = DateTime.Today.AddDays(1);

        [Parameter] public Guid Site {  get; set; }
        [Parameter] public double Offset { get; set; }
        [Parameter] public bool ShowPivotChar { get; set; }

        [Parameter]
        public EventCallback<DateTimeOffset> LastUpdateGridChanged { get; set; }

        private string INBOUND = @"<span class=""tag-inbound""><i class=""mlxPanel mlx-ico-entrada""></i> Inbound</span>";
        private string OUTBOUND = @"<span class=""tag-outbound""><i class=""mlxPanel mlx-ico-salida""></i> Outbound</span>";

        private List<FlowTypeDto> TypeBounds = new();

        private Guid? _lastSite;

        public override void Dispose()
        {
            _EventServices.Unsubscribe(ConstantComponents.LiveScheduleGridComponent, ConstantComponents.TopBarComponent, SettingsActions);
            base.Dispose();
        }

        protected override async Task OnInitializedAsync()
        {
            TranslateResoruce();
            TypeBounds = new List<FlowTypeDto> {
                    new FlowTypeDto { Key = false, Text = L.Loc("Inbound"), Html = (MarkupString)INBOUND },
                    new FlowTypeDto { Key = true,  Text = L.Loc("Outbound"), Html = (MarkupString)OUTBOUND }
                };

            await GetUserDataAsync();
            _EventServices.Subscribe(ConstantComponents.LiveScheduleGridComponent, ConstantComponents.TopBarComponent, SettingsActions);
        }

        protected void TranslateResoruce()
        {
            INBOUND = @"<span class=""tag-inbound""><i class=""mlxPanel mlx-ico-entrada""></i>" + L.Loc("Inbound") + "</span>";
            OUTBOUND = @"<span class=""tag-outbound""><i class=""mlxPanel mlx-ico-salida""></i> " + L.Loc("Outbound") + "</span>";
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_lastSite != Site && Site.ToString() != Guid.Empty.ToString())
            {
                _lastSite = Site;
                await ReloadSimulationData();
            }
        }

        public override async void SettingsActions(EventArguments args)
        {
            switch (args.EventActions)
            {                
                case EventActions.UpdateScheduleGrid:
                    await ReloadSimulationData();
                    break;
                default:
                    break;
            }
        }

        public async Task ReloadSimulationData()
        {
            _orderIssuesToastShown = false;
            userFormat.TimeZoneOffSet = Offset;
            var vehicleMetricsParent = await _SimulateService.GetSimulateData(
                Site, userFormat, Models.Enums.EnumViewPlanning.Trailer
            );

            VehicleMetrics = vehicleMetricsParent?.PlanningData?.VehicleMetrics
                  ?? new List<VehicleMetricsDto>();

            CheckAndShowOrderIssuesToast();
            var _lastUpdateGrid = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(Offset));
            await LastUpdateGridChanged.InvokeAsync(_lastUpdateGrid);
            ShowHidePanel(false, false);
        }

        private void OnCustomizeGridElement(GridCustomizeElementEventArgs e)
        {
           

            //// ---------- CELDAS ----------
            //if (e.ElementType == GridElementType.DataCell)
            //{
            //    var rowObj = e.Grid.GetDataItem(e.VisibleIndex);

            //    // MUY IMPORTANTE → DevExpress usa Column.Name, no FieldName
            //    var fieldName = e.Column?.Name;

            //    if (string.IsNullOrEmpty(fieldName))
            //        return;

            //    if (rowObj is VehicleMetricsDto vRow && fieldName == "metrics")
            //    {
            //        if (vRow.OTIF < 1)
            //        {
            //            e.CssClass += " mlx-ico-x-preview";
            //        }
            //        else
            //        {
            //            e.CssClass += " mlx-ico-check-preview";
            //        }
            //    }
            //}
        }

        DateTime GetChartStart(IEnumerable<VehicleMetricsDto> data)
        {
            if (data == null || !data.Any())
                return DayStart; // fallback seguro

            var min = data.Min(x => x.MaxEnd);
            return DateTime.SpecifyKind(new DateTime(min.Year, min.Month, min.Day, min.Hour, 0, 0),DateTimeKind.Unspecified);
        }

        DateTime GetChartEnd(IEnumerable<VehicleMetricsDto> data)
        {
            if (data == null || !data.Any())
                return DayEnd; // fallback seguro

            var max = data.Max(x => x.MaxEnd);
            return DateTime.SpecifyKind(new DateTime(max.Year, max.Month, max.Day, max.Hour, 0, 0)
                .AddHours(1), DateTimeKind.Unspecified);
        }

        double? GetOtfFilterValue(object filterValue)
        {
            if (filterValue is double d)
                return d * 100;   // 0–1 → 0–100

            return null;
        }

        void SetOtfFilterValue(GridDataColumnFilterRowCellTemplateContext context, double? value)
        {
            if (value == null)
            {
                context.FilterRowValue = null;
            }
            else
            {
                context.FilterRowValue = value / 100.0; // 0–100 → 0–1
            }
        }

        private bool _orderIssuesToastShown;

        private void CheckAndShowOrderIssuesToast()
        {
            var filtered = VehicleMetrics?
                .Where(x => x.OTIF < 1 && x.MaxEnd >= DateTime.UtcNow)
                .ToList() ?? new List<VehicleMetricsDto>();

            if (filtered.Any())
                return;

            if (_orderIssuesToastShown)
                return;

            _orderIssuesToastShown = true;

            ToastService.ShowToast(new ToastOptions
            {
                ProviderName = "Global",
                RenderStyle = ToastRenderStyle.Info,
                ThemeMode = ToastThemeMode.Light,
                Title = L.Loc("NOORDERINCIDENTS_TITLE"),
                Text = L.Loc("NOORDERINCIDENTS_DESCRIPTION"),
                
            });
        }

        private DateTime ConvertFromUtc(DateTime utc, double _offset)
        {
            if (utc.Kind != DateTimeKind.Utc)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            var offset = TimeSpan.FromHours(_offset);

            return DateTime.SpecifyKind(utc + offset, DateTimeKind.Unspecified);
        }


        public class ChartPoint
        {
            public DateTime Time { get; set; }
            public double OTIF { get; set; }
            public double Slack { get; set; }
            public double Delay { get; set; }
        }


        public class FlowTypeDto
        {
            public bool Key { get; set; }
            public string Text { get; set; }
            public MarkupString Html { get; set; }
        }

        public static string FormatMinutes(double minutes, bool showSeconds = false)
        {
            if (minutes < 0) minutes = 0;

            var totalSeconds = (int)Math.Round(minutes * 60, MidpointRounding.AwayFromZero);
            var time = TimeSpan.FromSeconds(totalSeconds);

            return showSeconds
                ? $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}"
                : $"{(int)time.TotalHours:00}:{time.Minutes:00}";
        }
    }
}
