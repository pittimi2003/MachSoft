using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Web.Helper;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Web.Components.SchedulerComponent
{
    public partial class MlxScheduler : ComponentBase, IDisposable
	{

        #region Fields

        DxSchedulerDataStorage DataStorage = new DxSchedulerDataStorage()
        {
            AppointmentMappings = new DxSchedulerAppointmentMappings()
            {
                Type = "AppointmentType",
                Start = "StartDate",
                End = "EndDate",
                ResourceId = "ResourceId"
            },
            ResourceMappings = new DxSchedulerResourceMappings()
            {
                Id = "Id",
                Caption = "Name",
                BackgroundCssClass = "BackgroundCss",
                TextCssClass = "TextCss"
            }
        };

        #endregion

        #region Properties

        [Parameter] public Guid CurrentPlanning { get; set; }

        [Parameter] public int HoursToShow { get; set; }
        public int CellMinWidth { get; set; } = 135;
        [Parameter] public IEnumerable<YardResourceBase> Resources { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime StartDateNow { get; set; }
		[Parameter]  public UserFormatOptions userFormat { get; set; } = new();
        public List<YardResourceBase> VisibleResources { get; set; }
        [Parameter] public Int16 ZoomLevel { get; set; } = 60;
        //20,30, 60
        [Parameter] public EventCallback<Int16> ZoomLevelChanged { get; set; }
		[Parameter] public bool LoadPanelVisible { get; set; } = false;
        [Inject] private NavigationManager NavigationManager { get; set; }
		[Parameter] public double Offset { get; set; } = 0;
		[Inject] private IJSRuntime JS { get; set; }
        [Inject] private ProtectedLocalStorage LocalStorage { get; set; }
		[Parameter] public DataTotalStages? Metrics { get; set; }
		[Parameter] public string? WarehouseName { get; set; }

        private ElementReference _schedulerContainer;

        #endregion

        #region Methods

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _eventServices.Subscribe(nameof(MlxSchedulerOptions), nameof(MlxSchedulerOptions), EventsTopBar);
		}

        private void EventsTopBar(EventArguments eventData)
        {
            try
            {
                switch (eventData.EventActions)
                {
                    case EventActions.ExportServicePDF:
                        OnExportServicePDFClick();
                        break;
					case EventActions.CollapsedExpandedScheduler:
						if (eventData.EventData is bool collasped)
							OnCollapsedExpandedClick(collasped);
						break;
                    case EventActions.ExportServiceExcel:
						OnExportServiceExcelClick();
						break;
					case EventActions.ExportServicePrint:
						OnExportServicePrintClick();
						break;
				}
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task OnExportServicePrintClick()
        {
			await JS.CallJs("scheduler", "printWholePage");			
        }

		public async Task OnExportServiceExcelClick()
        {
			var dayStart = StartDate.Date;
			var dayEnd = dayStart.AddDays(1);

			var model = new
			{
				title = "Yard Calendar",
				warehouseName = WarehouseName,
				start = StartDate,
				hours = 24,

				resources = Resources.Select(r => new
				{
					id = r.Id,
					name = r.Name,
					isGroup = r.IsGroup,
					isBlock = r.IsBlock,
					resourceType = r.ResourceType
				}).ToList(),

				totals = new
				{
					ST = Metrics.TS,
					UR = Metrics.AU,
					UP = Metrics.PU,
					CT = Metrics.TC,
					TotalSaturation = Metrics.TotalSaturation,
					ActualUtilization = Metrics.ActualUtilization,
					PlannedUtilization = Metrics.PlannedUtilization,
					TotalCapacity = Metrics.TotalCapacity
				},

				metrics = Resources
		        .SelectMany(r => r.Appointments
			        .Where(m => m.StartDate >= dayStart && m.StartDate < dayEnd)
			        .Select(m => new
			        {
				        resourceId = r.Id,
				        dateTime = m.StartDate,
				        totalSaturation = m.TotalSaturation,
				        actualUtilization = m.ActualUtilization,
				        plannedUtilization = m.PlannedUtilization,
				        totalCapacity = m.TotalCapacity
			        })
		        ).ToList()
            };

			await JS.CallJs("scheduler", "exportExcelCombined", model, ConstantFileNames.YardCalendarExcel, ",");
		}


		public async Task OnExportServicePDFClick()
        {
			var opts = new
			{
				filename = ConstantFileNames.YardCalendarPDF,
				format = "a4",
                horasPorHoja = 8,
                totalHoras = 24,
				orientation = "landscape",
				marginPt = 16,
				scale = 1,
				fitWidth = true
			};

			await JS.CallJs("scheduler", "exportPdfByHours", "GanttContainer", opts);
		}

        protected override void OnParametersSet()
        {
            if (HoursToShow == 24)
                CellMinWidth = 135;
            else if (HoursToShow == 16)
                CellMinWidth = 110;
            else if (HoursToShow == 12)
                CellMinWidth = 150;
            else
                CellMinWidth = 220;

            StartDate = DateTime.UtcNow.AddHours(Offset);
			StartDateNow = StartDate;
			StartDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 0, 0, 0);

			DataStorage.ResourcesSource = Resources;
            DataStorage.AppointmentsSource = Resources.SelectMany(resource => resource.Appointments).ToList();
            VisibleResources = Resources.Where(x => x.IsGroup).ToList();           
        }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
                await JS.InvokeVoidAsync("schedulerInterop.attachZoomHandler", _schedulerContainer,DotNetObjectReference.Create(this));
            }
			await UpdateNowMarkerPosition();
		}

		string GetColor(int percentage)
        {
            if (percentage < 70) return "var(--sp-color-heatmap-range-01)"; // Azul
            if (percentage < 90) return "var(--sp-color-heatmap-range-00)"; // Amarillo
            return "var(--sp-color-heatmap-range-04)"; // Rosa
        }

        private async Task OnAppointmentDoubleClick(YardResourceBase resource, AppointmentMetrics appointment)
        {
            string urlPage = $"{NavigationManager.BaseUri}live-schedule";

            YardResourceFilter filter = new()
            {
                EntityId = resource.EntityId,
                Name = resource.Name,
                StartDate = appointment.StartDate,
                EndDate = appointment.EndDate,
                ResourceType = resource switch
                {
                    DockResource => YardResourceType.Docks,
                    StageResource => YardResourceType.Stages,
                }
            };
            LocalStorage.SetAsync(StorageKeysConstants.YardResourceFilter, filter);
            await JS.InvokeVoidAsync("openUrlInNewTab", urlPage);
        }

        void ToggleGroup(int id)
        {
            var childResources = Resources
                .Where(r => r.ParentId == id && !r.IsGroup)
                .ToList();

            bool allVisible = childResources.All(d => VisibleResources.Contains(d));

            if (allVisible)
            {
                VisibleResources.RemoveAll(r => childResources.Contains(r));
            }
            else
            {
                foreach (var child in childResources)
                {
                    if (!VisibleResources.Contains(child))
                        VisibleResources.Add(child);
                }
            }

            VisibleResources = VisibleResources.OrderBy(r => r.Id).ToList();
        }

		private async Task UpdateNowMarkerPosition()
		{
			double cellWidth = 0;

            if (HoursToShow == 8)
                cellWidth = 220;
            else if (HoursToShow == 12)
                cellWidth = 150;
            else if (HoursToShow == 16)
                cellWidth = 110;
            else
                cellWidth = 135;

			var startHour = new DateTime(StartDateNow.Year, StartDateNow.Month, StartDateNow.Day, StartDateNow.Hour, 0, 0);
			var now = StartDateNow;
			var totalHours = (now - startHour).TotalHours;

			var markerLeft = ((StartDateNow.Hour * cellWidth) + (totalHours * cellWidth)) * (60 / ZoomLevel);
			var imageLeft = markerLeft - 7;

			await JS.InvokeVoidAsync("updateNowMarker", markerLeft, imageLeft);            
		}		

		public async Task OnCollapsedExpandedClick(bool isCollapsed)
		{
            if (isCollapsed)
                VisibleResources = Resources.ToList();
            else
                VisibleResources = Resources.Where(z => z.IsGroup).ToList();

            StateHasChanged();
        }

        [JSInvokable]
        public async Task ChangeZoom(bool zoomIn)
        {
			await JS.InvokeVoidAsync("saveSchedulerScroll", "YardCalendarId");
			if (zoomIn)
            {
                if (ZoomLevel == 20) ZoomLevel = 30;
                else if (ZoomLevel == 30) ZoomLevel = 60;
            }
            else
            {
                if (ZoomLevel == 60) ZoomLevel = 30;
                else if (ZoomLevel == 30) ZoomLevel = 20;
            }

            await ZoomLevelChanged.InvokeAsync(ZoomLevel);
			await JS.InvokeVoidAsync("restoreSchedulerScroll", "YardCalendarId");
			StateHasChanged(); // refresca el scheduler
        }


        public void Dispose()
        {
			_eventServices.Unsubscribe(nameof(MlxSchedulerOptions), nameof(MlxSchedulerOptions), EventsTopBar);
		}

        #endregion
    }
}
