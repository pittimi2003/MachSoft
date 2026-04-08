//using DevExpress.Blazor;
//using Microsoft.JSInterop;
//using Mss.WorkForce.Code.Models.DTO;
//using Mss.WorkForce.Code.Models.DTO.Enums;
//using Mss.WorkForce.Code.Models.DTO.Preview;
//using Mss.WorkForce.Code.Models.ModelGantt;
//using Mss.WorkForce.Code.Models.Models;
//using Mss.WorkForce.Code.Models.ModelUpdate;
//using Mss.WorkForce.Code.Web.Common;
//using Mss.WorkForce.Code.Web.Components.Gantt;
//using Mss.WorkForce.Code.Web.Components.Pages.Planning;
//using Mss.WorkForce.Code.Web.Components.Pages.Preview.Components;
//using Mss.WorkForce.Code.Web.Enums;
//using Mss.WorkForce.Code.Web.Model;
//using Mss.WorkForce.Code.Web.Services;
//using Newtonsoft.Json;

//namespace Mss.WorkForce.Code.Web.Components.Pages.Preview
//{
//	public partial class Preview : GanttOperations
//	{

//		#region Fields

//		private Temporalidad temporalityComponent;
//		private bool IsDirty { get; set; }
//		private bool IsDirtyWorkerRoles { get; set; }
//		private bool IsNotValidSchedule { get; set; } = true;
//		private bool HasTimeChanged { get; set; } = false;
//		private bool IsEditingInputProfile { get; set; }
//		private bool IsEditingOutputProfile { get; set; }
//		private bool ShowInputProfilesForm { get; set; }
//		private bool ShowOutputProfilesForm { get; set; }
//		private IReadOnlyList<object> _selectedInputProfiles;
//		private IReadOnlyList<object> _selectedOutputProfiles;
//		private List<LoadDto> OriginalInputProfilesList { get; set; }
//		private List<LoadDto> OriginalOutputProfilesList { get; set; }
//		private List<ShiftDto> OriginalShiftsDtos { get; set; }
//		private List<ResourceDto> OriginalWorkersDtos { get; set; }
//		private IReadOnlyList<object> SelectedInputProfiles
//		{
//			get => _selectedInputProfiles;
//			set
//			{
//				_selectedInputProfiles = value;
//				if (_selectedInputProfiles != null && _selectedInputProfiles?.Count > 0)
//					JS.InvokeVoidAsync("executeAction", "select", "idSectionInput");
//				else
//					JS.InvokeVoidAsync("executeAction", "unselect", "idSectionInput");
//			}
//		}
//		private IReadOnlyList<object> SelectedOutputProfiles
//		{
//			get => _selectedOutputProfiles;
//			set
//			{
//				_selectedOutputProfiles = value;
//				if (_selectedOutputProfiles != null && _selectedOutputProfiles?.Count > 0)
//					JS.InvokeVoidAsync("executeAction", "select", "idSectionOutput");
//				else
//					JS.InvokeVoidAsync("executeAction", "unselect", "idSectionOutput");
//			}
//		}
//		private IEnumerable<CatalogEntity>? ListRoles { get; set; }
//		private CatalogEntity? SelectedRole { get; set; }
//		private IEnumerable<CatalogEntity>? ListShift { get; set; }
//		private IEnumerable<CatalogEntity>? ListBreakProfiles { get; set; }
//		private CatalogEntity? SelectedShift { get; set; }
//		private List<ScheduleWorkerDto>? GridDataSchedulesOriginal { get; set; } = new();
//		private List<ScheduleWorkerDto>? GridDataSchedules { get; set; }
//		private List<ScheduleWorkerDto>? GridDataSchedulesEdit { get; set; }
//		private List<ScheduleWorkerDto>? GridDataSchedulesFilter { get; set; }
//		private bool showModalOperator;
//		private bool isAllSelected = false;
//		private bool isAllWorkersSelected = false;

//		private List<LoadDto> OldInputList { get; set; }
//		private List<LoadDto> OldOutputList { get; set; }

//		private TimeSpan OutputStarTime;
//		private TimeSpan OutputEndTime;
//		private TimeSpan InputStartTime;
//		private TimeSpan InputEndTime;
//		List<ResourceDto> Data = new List<ResourceDto>();
//		List<ResourceDto> DataOperator = new List<ResourceDto>();

//		private List<TemporalidadModel> temps = new List<TemporalidadModel>();

//		List<LoadDto> InputList = new List<LoadDto>();
//		List<LoadDto> OutputList = new List<LoadDto>();
//		List<LoadType> loadTypes = new List<LoadType>();
//		List<VehicleType> vehicleTypes = new List<VehicleType>();
//		List<ShiftDto> ShiftsDtos = new List<ShiftDto>();
//		List<ShiftDto> PopupShiftsDtos = new List<ShiftDto>();
//		List<ShiftSheduleDto> workerScheduleDtos = new List<ShiftSheduleDto>();
//		List<ShiftSheduleDto> SelectedWorkerSchedules = new List<ShiftSheduleDto>();

//		LoadDto InputDto { get; set; } = new();
//		LoadDto OutputDto { get; set; } = new();
//		ShiftDto ShiftDto { get; set; } = new();
//		ShiftDto copyShifDto { get; set; } = new();
//		ShiftDto CustomShiftDto { get; set; } = new();
//		ShiftDto copyCustomShifDto { get; set; } = new();

//		bool validInput { get; set; } = false;
//		bool validOutput { get; set; } = false;

//		private static TemporalidadModel defaultTemp;
//		bool isSaving { get; set; } = false;
//		private bool showModalEditSchedules = false;

//		private string btnInputCss => validInput ? "mlx-btn-action-enabled" : "mlx-btn-action-disabled";
//		private string btnOutputCss => validOutput ? "mlx-btn-action-enabled" : "mlx-btn-action-disabled";
//		private string btnEnabled => !isSaving ? "btn-mlx-blue-primary" : "btn-mlx-blue-primary";

//		private TemporalidadModel temporalidadSeleccionada { get; set; }

//		private List<object> SelectedData { get; set; } = new List<object>();
//		private Dictionary<string, string> ValidationErrors { get; set; } = new();

//		bool isAnyWorkerSelected { get; set; } = false;

//		public List<StopDto> AllStops { get; set; }
//		public List<WorkerStops> OriginalWorkers { get; set; }

//		public List<WorkerStops> AllWorkers { get; set; }

//		public bool HasBreakChange { get; set; } = false;

//		public GanttDataConvertDto<TaskData> DataSimulation = new();
//		public List<TaskData> GanttReadyData = new();
//		public GanttComponent<TaskData> GanttReference;
//		public GetAttributesDto<TaskData>? GridParameters { get; set; }

//		private string MessageErrorHour = "* Start cannot be greater than End";
//		private string MessageMandatoryField = "* Mandatory field";
//		private string CaptionSimulation = "Simulate";
//		private string CaptionRoles = "Roles";
//		private string CaptionOperators = "Operators";
//		private string CaptionEquipments = "Equipments";
//		private string CaptionEquipmentGroup = "Equipment Group";
//		private string NullSelecction = "Select...";
//		private string CaptionLoad = "Load";
//		private string CaptionVehicle = "Vehicle";
//		private string CaptionNVehicle = "N° Vehicle";
//		private string CaptionStart = "Start";
//		private string CaptionEnd = "End";
//		private string CaptionName = "Name";
//		private string CaptionShift = "Shift";
//		private string CaptionNumber = "Number";
//		private string CaptionBreakProfile = "Break profile";
//		private string CaptionStartShift = "Start Shift";
//		private string CaptionEndShift = "End Shift";
//		private string CaptionWorkHours = "Work Hours";
//		private string CaptionWorkers = "Workers";


//		#endregion

//		#region Methods

//		protected override async Task OnAfterRenderAsync(bool firstRender)
//		{
//			if (firstRender || RepaintDataGantt)
//			{
//				await LoadGanttServiceAsync();
//				GanttSetup.ChooserColumnDefault = true;
//				GanttSetup.value = new SimulationParametrizedDto()
//				{
//					inputList = InputList,
//					outputList = OutputList,
//					dataOperator = DataOperator,
//					shiftDtos = ShiftsDtos,
//					stops = AllStops
//				};
//			}

//			await RepaintGanttAsync(false, ShowInfoDrawer);
//		}

//		protected override async Task OnInitializedAsync()
//		{
//			await GetUserDataAsync();
//			ShowHidePanel(true, true);
//			GridParameters = new(l);
//			ShowInfoDrawer = true;
//			base.OnInitialized();
//			ShowHidePanel(true, true);
//			TranslateResoruces();
//			defaultTemp = new TemporalidadModel(userFormat.DateFormat) { Nombre = "DEFAULT", Data = "{}" };
//			Sites = _DataAccess.GetWarehouse(_DataAccess.GetOrganization().FirstOrDefault().Id)
//		  .Select(x => new SiteModel
//		  {
//			  Id = x.Id,
//			  Name = x.Name
//		  }).ToList();

//			CurrentSite = Sites.FirstOrDefault();
//			AllStops = new List<StopDto>();
//			LoadData();
//			GetListCatalogues();
//			GetGridDataSchedules();
//			ReloadOriginalList(CurrentSite.Id);
//			temporalidadSeleccionada = defaultTemp;
//			InputList = OriginalInputProfilesList.Select(o => (LoadDto)o.Clone()).ToList();
//			OutputList = OriginalOutputProfilesList.Select(o => (LoadDto)o.Clone()).ToList();
//			ShiftsDtos = OriginalShiftsDtos.Select(o => (ShiftDto)o.Clone()).ToList();
//			DataOperator = OriginalWorkersDtos.Select(o => (ResourceDto)o.Clone()).ToList();
//			GridDataSchedules = GridDataSchedulesOriginal.Select(o => (ScheduleWorkerDto)o.Clone()).ToList();
//			AllWorkers = OriginalWorkers.Select(o => (WorkerStops)o.Clone()).ToList();
//			LoadGanttSetupData();
//			ReloadTemporalityData(CurrentSite.Id);
//			Validate();

//		}

//		private void TranslateResoruces()
//		{
//			MessageErrorHour = l.Loc("* Start cannot be greater than End");
//			MessageMandatoryField = l.Loc("* Mandatory field");
//			CaptionSimulation = l.Loc("Simulate");
//			CaptionRoles = l.Loc("Roles");
//			CaptionOperators = l.Loc("Operators");
//			CaptionEquipments = l.Loc("Equipments");
//			CaptionEquipmentGroup = l.Loc("Equipment Group");
//			NullSelecction = l.Loc("Select...");
//			CaptionLoad = l.Loc("Load");
//			CaptionVehicle = l.Loc("Vehicle");
//			CaptionNVehicle = l.Loc("N° Vehicle");
//			CaptionStart = l.Loc("Start");
//			CaptionEnd = l.Loc("End");
//			CaptionName = l.Loc("Name");
//			CaptionShift = l.Loc("Shift");
//			CaptionNumber = l.Loc("Number");
//			CaptionBreakProfile = l.Loc("Break profile");
//			CaptionStartShift = l.Loc("Start Shift");
//			CaptionEndShift = l.Loc("End Shift");
//			CaptionWorkHours = l.Loc("Work Hours");
//			CaptionWorkers = l.Loc("Workers");
//			GanttProperties = GridParameters.GetTranslateCaptions();
//		}

//		public override Task OnSiteSeleccionadaChanged((SiteModel newSite, bool changeColumns) args)
//		{
//			CurrentSite = args.newSite;
//			ReloadTemporalityData(CurrentSite.Id);
//			temporalidadSeleccionada = temps.FirstOrDefault();
//			LoadData();
//			ReloadOriginalList(CurrentSite.Id);
//			GridDataSchedulesOriginal = new();
//			GetListCatalogues();
//			GetGridDataSchedules();
//			ResetConfigs();
//			UpdateConfigs();
//			HasBreakChange = false;
//			return base.OnSiteSeleccionadaChanged(args);
//		}

//		public override async Task LoadDataSimulation()
//		{
//			//         SeDataSimulation();
//			//         var parametrizedDto = GanttSetup.value as SimulationParametrizedDto;
//			//DataSimulation = await _SimulateService.GetSimulateDataPreview(GanttSetup.WarehouseId, parametrizedDto.inputList, parametrizedDto.outputList, parametrizedDto.dataOperator, parametrizedDto.shiftDtos, parametrizedDto.stops, userFormat);
//			//         //await PreparedData();
//			//         await base.LoadDataSimulation();
//		}

//		public override void LoadGanttSetupData(eViewPriority newView = eViewPriority.None)
//		{
//			GanttSetup.WarehouseId = CurrentSite.Id;
//			GanttSetup.typeGantt = Models.SignalR.GanttView.Preview;
//			GanttSetup.EditGantt = true;
//			GanttSetup.ShowToltip = false;
//			SeDataSimulation();
//		}

//		private void SeDataSimulation()
//		{
//			GanttSetup.value = new SimulationParametrizedDto()
//			{
//				inputList = InputList.Where(x => x.DataOperationType != OperationType.Delete).ToList(),
//				outputList = OutputList.Where(x => x.DataOperationType != OperationType.Delete).ToList(),
//				dataOperator = DataOperator,
//				shiftDtos = ShiftsDtos,
//				stops = AllStops
//			};
//		}

//		private bool HasInputRowErrors => InputList?.Where(p => p.DataOperationType != OperationType.Delete).Any(r => r.hour > r.endHour) == true;
//		private bool HasOutputRowErrors => OutputList?.Where(p => p.DataOperationType != OperationType.Delete).Any(r => r.hour > r.endHour) == true;

//		private bool IsInvalidShift(ShiftDto r) => r.initHour != default && r.endHour != default && r.initHour > r.endHour;

//		private async Task OnTemporalidadDateChanged(bool isDateChanged) => IsDirty = isDateChanged;

//		private async Task OnTemporalidadSeleccionadaChanged(TemporalidadModel newTemporalidad)
//		{
//			ShowHidePanel(false, false);
//			temporalidadSeleccionada = newTemporalidad;
//			HasBreakChange = false;
//			if (isSaving)
//			{
//				//await simulateService.SaveDataPreview(InputList, OutputList, ShiftsDtos, temporalidadSeleccionada, GridDataSchedules);
//				await _SimulateService.SaveDataPreview(InputList.Where(x => x.DataOperationType != OperationType.None).ToList(), OutputList.Where(x => x.DataOperationType != OperationType.None).ToList(), ShiftsDtos, temporalidadSeleccionada, GridDataSchedules, CurrentSite.Id, AllStops);
//				ReloadTemporalityData(CurrentSite.Id);
//				temporalidadSeleccionada.Data = temps.FirstOrDefault(x => x.Id == temporalidadSeleccionada.Id)?.Data;
//				isSaving = false;
//			}
//			GridDataSchedulesOriginal = new();
//			GetGridDataSchedules();
//			ResetConfigs();
//			IsDirty = false;
//			if (temporalidadSeleccionada.Nombre != "DEFAULT")
//				UpdateConfigs();
//			else
//				AllStops = new List<StopDto>();
//			await InvokeAsync(StateHasChanged); // Asegúrate de actualizar el estado.
//		}

//		private async Task OnTemporalidadSeleccionadaRemoved(TemporalidadModel newTemporalidad)
//		{
//			ShowHidePanel(false, false);
//			_DataAccess.RemoveTemporality(newTemporalidad.Id);
//			ResetConfigs();
//			GridDataSchedulesOriginal = new();
//			GetGridDataSchedules();
//			temporalidadSeleccionada = temps.First(x => x.Nombre == "DEFAULT");
//			UpdateConfigs();
//			IsDirty = IsDirtyModel();
//		}

//		private void ReloadOriginalList(Guid siteId)
//		{
//			OriginalInputProfilesList = ProcessLoadService.Process(_DataAccess.GetInboundLoad(siteId, GetCurrentConfig()), false);
//			OriginalOutputProfilesList = ProcessLoadService.Process(_DataAccess.GetOutboundLoad(siteId, GetCurrentConfig()), true);
//			OriginalShiftsDtos = ProcessLoadService.ProcessShifts(_DataAccess.GetShifts(siteId)).OrderBy(s => s.name).ToList();
//			OriginalWorkersDtos = ProcessLoadService.ProcessUpdateWorkers(_DataAccess.GetWorkersByProcess(siteId));
//			OriginalWorkers = _DataAccess.GetWorkersBySchedule(siteId);
//		}

//		private Guid GetCurrentConfig()
//		{
//			try
//			{
//				Guid configGui = Guid.Empty;
//				if (temporalidadSeleccionada is TemporalidadModel temporalidad && temporalidad.Nombre != "DEFAULT")
//					configGui = temporalidad.Id;

//				return configGui;
//			}
//			catch
//			{
//				return Guid.Empty;
//			}
//		}

//		private void ReloadTemporalityData(Guid siteId)
//		{
//			temps.Clear();
//			temps.Add(defaultTemp);
//			temps.AddRange(_DataAccess.GetConfigurationSequencesByWarehouse(siteId)
//			.Select(x => new TemporalidadModel(userFormat.DateFormat) { Id = x.Id, Nombre = x.Code, Data = x.Data, WarehouseId = x.WarehouseId, FechaDesde = x.ConfigurationSequences.Min(m => m.Date).Date, FechaHasta = x.ConfigurationSequences.Max(m => m.Date).Date })
//			.ToList());
//		}

//		private async void CancelDataPreview()
//		{
//			if (OldInputList != null)
//				InputList = OldInputList.Select(o => (LoadDto)o.Clone()).ToList();
//			if (OldOutputList != null)
//				OutputList = OldOutputList.Select(o => (LoadDto)o.Clone()).ToList();
//			if (OriginalShiftsDtos != null)
//				ShiftsDtos = OriginalShiftsDtos.Select(o => (ShiftDto)o.Clone()).ToList();
//			ReloadTemporalityData(CurrentSite.Id);
//			temporalidadSeleccionada = temps.FirstOrDefault(x => x.Id == temporalidadSeleccionada.Id);
//			IsDirty = false;
//			InvokeAsync(StateHasChanged);
//		}

//		private async void SaveDataPreview()
//		{
//			isSaving = true;
//			if (temporalidadSeleccionada.Nombre == "DEFAULT")
//			{
//				temporalityComponent.AbrirModalCrear();
//				return;
//			}
//			try
//			{
//				await _SimulateService.SaveDataPreview(InputList.Where(x => x.DataOperationType != OperationType.None).ToList(), OutputList.Where(x => x.DataOperationType != OperationType.None).ToList(), ShiftsDtos, temporalidadSeleccionada, GridDataSchedules.Where(x => x.DataOperationType != OperationType.None).ToList(), CurrentSite.Id, AllStops);
//				SaveTemporalityDate();
//				ReloadTemporalityData(CurrentSite.Id);
//				isSaving = false;
//				IsDirty = false;
//				IsEditingInputProfile = false;
//				IsEditingOutputProfile = false;
//				InvokeAsync(StateHasChanged);
//			}
//			catch (Exception ex)
//			{
//				isSaving = false;
//			}

//		}

//		private void SaveTemporalityDate()
//		{
//			var fechaDesde = _DataAccess.GetSequenceByConfigurationSequence(temporalidadSeleccionada.Id).FirstOrDefault();
//			var fechaHasta = _DataAccess.GetSequenceByConfigurationSequence(temporalidadSeleccionada.Id).LastOrDefault();
//			int totalDays = (temporalidadSeleccionada.FechaHasta - temporalidadSeleccionada.FechaDesde).Days;

//			var sequences = _DataAccess.GetSequenceByConfigurationSequence(temporalidadSeleccionada.Id).ToList();

//			if (fechaHasta.Date.Day < temporalidadSeleccionada.FechaHasta.Day && fechaDesde.Date.Day == temporalidadSeleccionada.FechaDesde.Day)
//				AddConfigurationSequence(totalDays);
//			else
//			{
//				AddConfigurationSequence(totalDays);
//				DeleteConfigurationSequence(fechaHasta.Sequence, sequences);
//			}
//		}

//		private void AddConfigurationSequence(int totalDays)
//		{
//			for (int i = 0; i <= totalDays; i++)
//			{
//				var newSequence = new ConfigurationSequence
//				{
//					Id = Guid.NewGuid(),
//					Date = temporalidadSeleccionada.FechaDesde.AddDays(i),
//					Sequence = 0,
//					ConfigurationSequenceHeaderId = temporalidadSeleccionada.Id,
//					ConfigurationSequenceHeader = null,
//				};
//				_DataAccess.AddConfigurationSequence(newSequence);
//			}
//		}

//		private void DeleteConfigurationSequence(int totalDays, IEnumerable<ConfigurationSequence> configurationSequences)
//		{
//			foreach (var item in configurationSequences)
//				_DataAccess.RemoveConfigurationSequence(item.Id);
//		}

//		private void UpdateConfigs()
//		{
//			var changes = JsonConvert.DeserializeObject<Get>(temporalidadSeleccionada.Data);
//			ExecuteNews(changes.New);
//			ExecuteUpdates(changes.Update);
//			ExecuteDeletes(changes.Delete);
//		}

//		private void ExecuteNews(New news)
//		{
//			var workers = _ContextConfig.GetWorkers();

//			foreach (var oS in news?.OrderSchedule ?? new List<OrderScheduleUpdate>())
//			{
//				if (oS.IsOut == true)
//				{
//					OutputList.Add(new LoadDto
//					{
//						id = oS.Id,
//						load = _DataAccess.GetLoadProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.LoadId).Name,
//						vehicle = _DataAccess.GetVehicleProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.VehicleId).Name,
//						numberVehicle = oS.NumberVehicles.Value,
//						hour = oS.InitHour.Value,
//						endHour = oS.EndHour.Value,
//						DataOperationType = OperationType.Insert,
//					});
//				}
//				if (oS.IsOut == false)
//				{
//					InputList.Add(new LoadDto
//					{
//						id = oS.Id,
//						load = _DataAccess.GetLoadProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.LoadId).Name,
//						vehicle = _DataAccess.GetVehicleProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.VehicleId).Name,
//						numberVehicle = oS.NumberVehicles.Value,
//						hour = oS.InitHour.Value,
//						endHour = oS.EndHour.Value,
//						DataOperationType = OperationType.Insert,
//					});
//				}
//			}



//			foreach (var s in news?.Shift ?? new List<ShiftUpdate>())
//			{
//				ShiftsDtos.Add(new ShiftDto
//				{
//					name = s.Name,
//					initHour = TimeSpan.FromHours(s.InitHour.Value),
//					endHour = TimeSpan.FromHours(s.EndHour.Value)
//				});
//			}

//			foreach (var entity in news?.Schedule ?? new List<ScheduleUpdate>())
//			{
//				if (GridDataSchedules.FirstOrDefault(x => x.WorkerId == _ContextConfig.GetAvailableWorkers().Where(m => m.Id == entity.AvailableWorkerId).Select(m => m.WorkerId).First()) is ScheduleWorkerDto workerDto)
//				{
//					workerDto.BreakProfileId = entity.BreakProfileId;
//					workerDto.ShiftId = entity.ShiftId;
//					workerDto.Id = entity.Id;
//					workerDto.AvailableWorkerId = entity.AvailableWorkerId;
//					workerDto.DataOperationType = OperationType.Insert;
//				}
//			}

//			AllStops = new List<StopDto>();
//			foreach (var entity in news?.Stops ?? new List<StopDto>())
//			{
//				AllStops.Add(entity);
//			}
//		}

//		private void ExecuteUpdates(Update updates)
//		{
//			try
//			{
//				foreach (var oS in updates?.OrderSchedule ?? new List<OrderScheduleUpdate>())
//				{
//					if (oS.IsOut == true)
//					{
//						OutputList.Where(x => x.id == oS.Id).ToList().ForEach(o =>
//						{
//							o.id = oS.Id;
//							o.load = oS.LoadId != null ? _DataAccess.GetLoadProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.LoadId).Name : o.load;
//							o.vehicle = oS.VehicleId != null ? _DataAccess.GetVehicleProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.VehicleId).Name : o.vehicle;
//							o.numberVehicle = oS.NumberVehicles != null ? oS.NumberVehicles.Value : o.numberVehicle;
//							o.hour = oS.InitHour != null ? oS.InitHour.Value : o.hour;
//							o.endHour = oS.EndHour != null ? oS.EndHour.Value : o.endHour;
//							o.DataOperationType = OperationType.Update;
//						});
//					}
//					if (oS.IsOut == false)
//					{
//						InputList.Where(x => x.id == oS.Id).ToList().ForEach(o =>
//						{
//							o.id = oS.Id;
//							o.load = oS.LoadId != null ? _DataAccess.GetLoadProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.LoadId).Name : o.load;
//							o.vehicle = oS.VehicleId != null ? _DataAccess.GetVehicleProfiles(CurrentSite.Id).FirstOrDefault(x => x.Id == oS.VehicleId).Name : o.vehicle;
//							o.numberVehicle = oS.NumberVehicles != null ? oS.NumberVehicles.Value : o.numberVehicle;
//							o.hour = oS.InitHour != null ? oS.InitHour.Value : o.hour;
//							o.endHour = oS.EndHour != null ? oS.EndHour.Value : o.endHour;
//							o.DataOperationType = OperationType.Update;
//						});
//					}
//				}
//				foreach (var s in updates?.Shift ?? new List<ShiftUpdate>())
//				{
//					ShiftsDtos.Where(x => x.id == s.Id).ToList().ForEach(x =>
//					{
//						x.name = !string.IsNullOrEmpty(s.Name) ? s.Name : x.name;
//						x.initHour = s.InitHour != null ? TimeSpan.FromHours(s.InitHour.Value) : x.initHour;
//						x.endHour = s.EndHour != null ? TimeSpan.FromHours(s.EndHour.Value) : x.endHour;
//					});
//				}


//				foreach (var entity in updates?.Schedule ?? new List<ScheduleUpdate>())
//				{
//					if (GridDataSchedules.FirstOrDefault(w => w.AvailableWorkerId == entity.AvailableWorkerId) is ScheduleWorkerDto schedule)
//					{
//						schedule.ShiftId = entity.ShiftId;
//						schedule.BreakProfileId = entity.BreakProfileId;
//						schedule.DataOperationType = OperationType.Update;

//						if (entity.CustomInitHour.HasValue)
//						{
//							if (ShiftsDtos.Where(x => x.id == entity.ShiftId).FirstOrDefault() != null)
//							{
//								if (ShiftsDtos.Where(x => x.id == entity.ShiftId).FirstOrDefault().schedules
//									.Where(x => x.workerId == schedule.AvailableWorkerId).FirstOrDefault() != null)
//								{
//									ShiftsDtos.Where(x => x.id == entity.ShiftId).FirstOrDefault().schedules
//										.Where(x => x.workerId == schedule.AvailableWorkerId).FirstOrDefault().initHour = DateTime.Today + TimeSpan.FromHours(entity.CustomInitHour ?? 0);
//									ShiftsDtos.Where(x => x.id == entity.ShiftId).FirstOrDefault().schedules
//										.Where(x => x.workerId == schedule.AvailableWorkerId).FirstOrDefault().endHour = DateTime.Today + TimeSpan.FromHours(entity.CustomEndHour ?? 0);
//								}
//							}
//						}
//					}

//				}
//			}
//			catch (Exception ex)
//			{
//				Logger.LogError($"Error {ex.Message}");
//			}

//		}

//		private void ExecuteDeletes(Delete deletes)
//		{
//			var schedules = _ContextConfig.GetSchedule(temporalidadSeleccionada.Id);
//			var workers = _ContextConfig.GetWorkers();

//			foreach (var oS in deletes?.OrderSchedule ?? new List<OrderScheduleUpdate>())
//			{
//				if (OutputList.FirstOrDefault(x => x.id == oS.Id) is LoadDto elementInput)
//					elementInput.DataOperationType = OperationType.Delete;

//				if (InputList.FirstOrDefault(x => x.id == oS.Id) is LoadDto elementOutPut)
//					elementOutPut.DataOperationType = OperationType.Delete;
//			}
//			foreach (var s in deletes?.Shift ?? new List<ShiftUpdate>())
//			{
//				ShiftsDtos = ShiftsDtos.Where(x => x.id != s.Id).ToList();
//			}

//			foreach (var aw in deletes?.Schedule ?? new List<ScheduleUpdate>())
//			{
//				if (schedules?.FirstOrDefault(x => x.Id == aw.Id).AvailableWorker is AvailableWorker availableWorker && workers?.FirstOrDefault(x => x.Id == availableWorker.WorkerId) is Worker worker)
//				{
//					if (DataOperator.FirstOrDefault(x => x.resource == worker.Rol.Name) is ResourceDto resourceRol)
//						resourceRol.value = (int.Parse(resourceRol.value) - 1).ToString();

//					if (GridDataSchedules.FirstOrDefault(w => w.WorkerId == worker.Id) is ScheduleWorkerDto schedule)
//					{
//						schedule.ShiftId = null;
//						schedule.BreakProfileId = null;
//						schedule.DataOperationType = OperationType.Delete;
//					}
//				}
//			}
//		}

//		private void LoadData()
//		{
//			Data = ProcessLoadService.ProcessUpdateEquipments(_DataAccess.GetEquipmentTypes(CurrentSite.Id));
//			DataOperator = ProcessLoadService.ProcessUpdateWorkers(_DataAccess.GetWorkersByProcess(CurrentSite.Id));
//			loadTypes = ProcessLoadService.ProcessLoadTypes(_DataAccess.GetLoadProfiles(CurrentSite.Id));
//			vehicleTypes = ProcessLoadService.ProcessVehicleTypes(_DataAccess.GetVehicleProfiles(CurrentSite.Id));
//		}

//		private void ResetConfigs()
//		{
//			InputList = OriginalInputProfilesList.Select(o => (LoadDto)o.Clone()).ToList();
//			OutputList = OriginalOutputProfilesList.Select(o => (LoadDto)o.Clone()).ToList();
//			ShiftsDtos = OriginalShiftsDtos.Select(o => (ShiftDto)o.Clone()).ToList();
//			DataOperator = OriginalWorkersDtos.Select(o => (ResourceDto)o.Clone()).ToList();
//			GridDataSchedules = GridDataSchedulesOriginal.Select(o => (ScheduleWorkerDto)o.Clone()).ToList();
//			AllWorkers = OriginalWorkers.Select(o => (WorkerStops)o.Clone()).ToList();
//		}

//		private void AddInput()
//		{

//			InputList.Add(new LoadDto()
//			{
//				id = Guid.NewGuid(),
//				load = InputDto.load,
//				vehicle = InputDto.vehicle,
//				hour = InputDto.hour,
//				endHour = InputDto.endHour,
//				numberVehicle = InputDto.numberVehicle,
//				DataOperationType = OperationType.Insert
//			});

//			InputDto = new();
//			validInput = false;
//			JS.InvokeVoidAsync("enableActionButton", "idSectionInput");
//		}

//		private void AddOutput()
//		{

//			OutputList.Add(new LoadDto()
//			{
//				id = Guid.NewGuid(),
//				load = OutputDto.load,
//				vehicle = OutputDto.vehicle,
//				hour = OutputDto.hour,
//				endHour = OutputDto.endHour,
//				numberVehicle = OutputDto.numberVehicle,
//				DataOperationType = OperationType.Insert
//			});

//			OutputDto = new();
//			validOutput = false;
//			JS.InvokeVoidAsync("enableActionButton", "idSectionOutput");
//		}

//		public void DeleteOutputElement(Guid id)
//		{
//			if (OutputList.FirstOrDefault(x => x.id == id) is LoadDto element)
//				OutputList.Remove(element);
//		}

//		public void ValidInputOperation() => validInput = (!string.IsNullOrEmpty(InputDto.load) && !string.IsNullOrEmpty(InputDto.vehicle) && !string.IsNullOrEmpty(InputDto.numberVehicle.ToString()) && !string.IsNullOrEmpty(InputDto.hour.ToString()) && InputDto.numberVehicle != 0 && InputDto.hour <= InputDto.endHour);

//		public void ValidOutputOperation() => validOutput = (!string.IsNullOrEmpty(OutputDto.load) && !string.IsNullOrEmpty(OutputDto.vehicle) && !string.IsNullOrEmpty(OutputDto.numberVehicle.ToString()) && !string.IsNullOrEmpty(OutputDto.hour.ToString()) && OutputDto.numberVehicle != 0 && OutputDto.hour <= OutputDto.endHour);

//		private void EditingGridDataItems(string idSection, PanelType panel)
//		{
//			LoadPanelVisible = false;
//			BackGroudShading = false;
//			switch (panel)
//			{
//				case PanelType.InputProfiles:
//					IsEditingInputProfile = true;
//					OldInputList = InputList.Select(o => (LoadDto)o.Clone()).ToList();
//					break;
//				case PanelType.OutputProfiles:
//					IsEditingOutputProfile = true;
//					OldOutputList = OutputList.Select(o => (LoadDto)o.Clone()).ToList();
//					break;
//			}
//			JS.InvokeVoidAsync("executeAction", "edit", idSection);
//		}

//		private async Task SetInputProfileToReadOnly()
//		{
//			bool hasChanges = !InputList.SequenceEqual(OldInputList);

//			if (hasChanges)
//			{
//				bool confirmDiscard = await _DialogService.ShowDialogAsync("Notification", $"You have unsaved changes. Are you sure you want to leave and discard them?");

//				if (confirmDiscard)
//					InputList = OldInputList.Select(o => (LoadDto)o.Clone()).ToList();
//				else
//					return;
//			}

//			IsEditingInputProfile = false;
//			ShowInputProfilesForm = false;
//			SelectedInputProfiles = null;
//			InputDto = new();
//			validInput = false;
//			IsDirty = IsDirtyModel();
//			await JS.InvokeVoidAsync("executeAction", "readOnly", "idSectionInput");

//			StateHasChanged();
//		}

//		private async Task SetOutputProfileToReadOnly()
//		{
//			bool hasChanges = !OutputList.SequenceEqual(OldOutputList);

//			if (hasChanges)
//			{
//				bool confirmDiscard = await _DialogService.ShowDialogAsync("Notification", $"You have unsaved changes. Are you sure you want to leave and discard them?");

//				if (confirmDiscard)
//					OutputList = OldOutputList.Select(o => (LoadDto)o.Clone()).ToList();
//				else
//					return;
//			}

//			IsEditingOutputProfile = false;
//			ShowOutputProfilesForm = false;
//			SelectedOutputProfiles = null;
//			OutputDto = new();
//			validOutput = false;
//			IsDirty = IsDirtyModel();
//			await JS.InvokeVoidAsync("executeAction", "readOnly", "idSectionOutput");

//			StateHasChanged();
//		}

//		private void SaveInputProfileElements()
//		{
//			foreach (var element in InputList.Where(o => o.DataOperationType == OperationType.None))
//			{
//				if (OldInputList.FirstOrDefault(x => x.id == element.id) is LoadDto oldElement)
//					element.DataOperationType = element.Equals(oldElement) ? element.DataOperationType : OperationType.Update;
//			}

//			IsEditingInputProfile = false;
//			ShowInputProfilesForm = false;
//			SelectedInputProfiles = null;
//			InputDto = new();
//			validInput = false;
//			//OriginalInputProfilesList = InputList.Select(o => (LoadDto)o.Clone()).ToList();
//			IsDirty = IsDirtyModel();
//			JS.InvokeVoidAsync("executeAction", "readOnly", "idSectionInput");
//			StateHasChanged();
//		}

//		private void SaveOutputProfileElements()
//		{
//			foreach (var element in OutputList.Where(o => o.DataOperationType == OperationType.None))
//			{
//				if (OldOutputList.FirstOrDefault(x => x.id == element.id) is LoadDto oldElement)
//					element.DataOperationType = element.Equals(oldElement) ? element.DataOperationType : OperationType.Update;
//			}

//			IsEditingOutputProfile = false;
//			ShowOutputProfilesForm = false;
//			SelectedOutputProfiles = null;
//			OutputDto = new();
//			validOutput = false;
//			//OriginalOutputProfilesList = OutputList.Select(o => (LoadDto)o.Clone()).ToList();
//			IsDirty = IsDirtyModel();
//			JS.InvokeVoidAsync("executeAction", "readOnly", "idSectionOutput");
//			StateHasChanged();
//		}

//		private async Task RemoveInputProfileElements()
//		{
//			if (SelectedInputProfiles?.Count > 0)
//			{
//				bool resp = await _DialogService.ShowDialogAsync("Notification", $"Are you sure you want to delete {SelectedInputProfiles?.Count} items?");
//				if (resp)
//				{
//					foreach (var order in SelectedInputProfiles.OfType<LoadDto>())
//					{
//						if (order.DataOperationType == OperationType.Insert)
//						{
//							InputList.Remove(order);
//							OldInputList.Remove(order);
//						}
//						else
//						{
//							if (InputList.FirstOrDefault(o => o.id == order.id) is LoadDto curentData)
//								curentData.DataOperationType = OperationType.Delete;

//							if (OldInputList.FirstOrDefault(o => o.id == order.id) is LoadDto oldData)
//								oldData.DataOperationType = OperationType.Delete;
//						}
//					}

//					SelectedInputProfiles = null;
//					await JS.InvokeVoidAsync("executeAction", "unselect", "idSectionInput");
//				}
//			}
//			StateHasChanged();
//		}

//		private async Task RemoveOutputProfileElements()
//		{
//			if (SelectedOutputProfiles?.Count > 0)
//			{
//				bool resp = await _DialogService.ShowDialogAsync("Notification", $"Are you sure you want to delete {SelectedOutputProfiles?.Count} items?");
//				if (resp)
//				{
//					foreach (var order in SelectedOutputProfiles.OfType<LoadDto>())
//					{
//						if (order.DataOperationType == OperationType.Insert)
//						{
//							OutputList.Remove(order);
//							OldOutputList.Remove(order);
//						}
//						else
//						{
//							if (OutputList.FirstOrDefault(o => o.id == order.id) is LoadDto curentData)
//								curentData.DataOperationType = OperationType.Delete;

//							if (OldOutputList.FirstOrDefault(o => o.id == order.id) is LoadDto oldData)
//								oldData.DataOperationType = OperationType.Delete;
//						}
//					}

//					SelectedOutputProfiles = null;
//					await JS.InvokeVoidAsync("executeAction", "unselect", "idSectionOutput");
//				}
//			}
//			StateHasChanged();
//		}

//		private void ShowAddInputProfiles()
//		{
//			ShowHidePanel(false, false);
//			IsEditingInputProfile = false;
//			ShowInputProfilesForm = true;
//			OldInputList = InputList.Select(o => (LoadDto)o.Clone()).ToList();
//			JS.InvokeVoidAsync("executeAction", "add", "idSectionInput");
//		}

//		private void ShowAddOutputProfiles()
//		{
//			ShowHidePanel(false, false);
//			IsEditingOutputProfile = false;
//			ShowOutputProfilesForm = true;
//			OldOutputList = OutputList.Select(o => (LoadDto)o.Clone()).ToList();
//			JS.InvokeVoidAsync("executeAction", "add", "idSectionOutput");
//		}

//		private void SelectedInputProfilesItems(IReadOnlyList<object> selectedItems) => SelectedInputProfiles = selectedItems;

//		private void SelectedOutputProfilesItems(IReadOnlyList<object> selectedItems) => SelectedOutputProfiles = selectedItems;

//		private bool IsDirtyModel()
//		{
//			return InputList.Any(p => p.DataOperationType != OperationType.None) ||
//				   OutputList.Any(p => p.DataOperationType != OperationType.None) ||
//				   ShiftsDtos.Any(p => p.DataOperationType != OperationType.None) ||
//				   HasBreakChange ||
//				   HasTimeChanged ||
//				   IsDirtyWorkerRoles;
//		}

//		private void GetListCatalogues()
//		{
//			Guid warehouseId = CurrentSite.Id;

//			ListRoles = _ContextConfig.GetRoles(warehouseId).Select(item => new CatalogEntity(string.Empty)
//			{
//				Id = item.Id,
//				Name = item.Name
//			});

//			ListShift = _ContextConfig.GetShifts(warehouseId).Select(item => new CatalogEntity(string.Empty)
//			{
//				Id = item.Id,
//				Name = item.Name
//			});

//			ListBreakProfiles = _ContextConfig.GetBreakProfiles(warehouseId).Select(item => new CatalogEntity(string.Empty)
//			{
//				Id = item.Id,
//				Name = item.Name
//			});
//		}

//		private void GetGridDataSchedules()
//		{
//			// Obtener los workers que tienes schedules
//			var scheduleWorkes = _DataAccess.GetOperators(CurrentSite.Id);

//			// Realizo la referencia cruzada con todos los workers
//			foreach (var worker in _DataAccess.GetAllWorkers(CurrentSite.Id))
//			{
//				ScheduleWorkerDto scheduleWorker = new();

//				scheduleWorker.WorkerId = worker.Id;
//				scheduleWorker.WorkerName = worker.Name;
//				scheduleWorker.WorkerNumber = worker.WorkerNumber ?? 0;
//				scheduleWorker.RolId = worker.RolId;

//				if (scheduleWorkes.FirstOrDefault(x => x.AvailableWorker.WorkerId == scheduleWorker.WorkerId) is Schedule schedule)
//				{
//					scheduleWorker.Id = schedule.Id;
//					scheduleWorker.Name = schedule.Name;

//					if (schedule.BreakProfile != null)
//						scheduleWorker.BreakProfileId = schedule.BreakProfileId;

//					if (schedule.Shift != null)
//						scheduleWorker.ShiftId = schedule.ShiftId;

//					if (schedule.AvailableWorker != null)
//						scheduleWorker.AvailableWorkerId = schedule.AvailableWorkerId;
//				}

//				GridDataSchedulesOriginal.Add(scheduleWorker);
//			}
//		}

//		private void OnSelectedItemChangeRole(CatalogEntity? role)
//		{
//			if (role != null)
//				UpdateGridWorkes(role.Id, SelectedShift?.Id);
//		}

//		private void OnSelectedItemChangeShift(CatalogEntity? shift)
//		{
//			if (shift != null)
//				UpdateGridWorkes(SelectedRole?.Id, shift.Id);
//		}

//		private void OnSelectedChangeWorker(ScheduleWorkerDto item, bool newValue)
//		{
//			if (SelectedShift != null && newValue)
//			{
//				var breakProfile = ListBreakProfiles.FirstOrDefault();

//				item.ShiftId = SelectedShift.Id;
//				item.BreakProfileId = breakProfile.Id;
//				item.AvailableWorkerId = item.AvailableWorkerId == null ?
//				_ContextConfig.GetAvailableWorkers().Where(m => m.WorkerId == item.WorkerId).Any() ?
//				_ContextConfig.GetAvailableWorkers().First(m => m.WorkerId == item.WorkerId).Id : Guid.NewGuid() :
//				item.AvailableWorkerId;
//				// Si es empty se considera como un nuevo schedule
//				if (item.Id == Guid.Empty)
//				{
//					item.Id = Guid.NewGuid();
//					item.DataOperationType = OperationType.Insert;
//				}
//				else if (GridDataSchedules.Any(item => item.Id == item.Id && item.DataOperationType == OperationType.Delete))
//				{
//					item.DataOperationType = OperationType.Insert;
//				}
//				else
//					item.DataOperationType = OperationType.Update;
//			}
//			else
//			{
//				item.ShiftId = null;
//				item.BreakProfileId = null;
//				item.DataOperationType = OperationType.Delete;
//			}

//			IsDirtyWorkerRoles = true;
//		}

//		private void OnSelectedItemChangeSchedule(SelectedDataItemChangedEventArgs<CatalogEntity>? data, ScheduleWorkerDto schedulePreviewDto)
//		{
//			if (data.ChangeSource == SelectionChangeSource.UserAction && schedulePreviewDto.Id != Guid.Empty)
//			{
//				schedulePreviewDto.DataOperationType = OperationType.Update;
//				IsDirtyWorkerRoles = true;
//			}
//		}

//		private void OnResourceSelectAllChanged(bool isChecked)
//		{
//			var breakProfile = ListBreakProfiles.FirstOrDefault();

//			foreach (var schedule in GridDataSchedulesFilter)
//			{
//				if (isChecked)
//				{
//					schedule.ShiftId = SelectedShift?.Id;
//					schedule.BreakProfileId = breakProfile.Id;
//				}
//				else
//				{
//					schedule.ShiftId = null;
//					schedule.BreakProfileId = null;
//				}

//			}
//			isAllSelected = isChecked;
//			StateHasChanged();
//		}

//		private void OnShowModalResource()
//		{
//			ShowHidePanel(false, false);
//			GridDataSchedulesEdit = GridDataSchedules.Select(o => (ScheduleWorkerDto)o.Clone()).ToList();
//			SelectedRole = null;
//			SelectedShift = null;
//			GridDataSchedulesFilter = null;
//			showModalOperator = true;
//			IsDirtyWorkerRoles = false;

//		}

//		private void UpdateGridWorkes(Guid? idRole, Guid? idShift)
//		{
//			if (idRole != null && idShift != null)
//			{
//				GridDataSchedulesFilter = GridDataSchedulesEdit?.Where(x => (x.RolId == idRole && x.ShiftId == idShift) ||
//														  (x.RolId == idRole && x.ShiftId == null)).ToList();
//			}
//		}

//		private void OnApplyChangesResources()
//		{
//			GridDataSchedules = GridDataSchedulesEdit?.Select(o => (ScheduleWorkerDto)o.Clone()).ToList();
//			showModalOperator = false;
//			IsDirty = true;
//		}

//		private void ShowModalEditSchedule()
//		{
//			ShowHidePanel(false, false);
//			showModalEditSchedules = true;
//			PopupShiftsDtos = ShiftsDtos.Select(o => (ShiftDto)o.DeepClone()).ToList();
//		}

//		private void HiddeModalEditSchedule()
//		{
//			ShiftDto = new();

//			HasBreakChange = false;

//			SelectedData.Clear();

//			HasTimeChanged = false;

//			PopupShiftsDtos.Clear();

//			StateHasChanged();
//			showModalEditSchedules = false;
//		}

//		private void UpdateSchedules()
//		{
//			ShiftsDtos = PopupShiftsDtos;

//			foreach (var element in ShiftsDtos.Where(o => o.DataOperationType == OperationType.None))
//			{
//				if (OriginalShiftsDtos.FirstOrDefault(x => x.id == element.id) is ShiftDto oldElement)
//					element.DataOperationType = element.Equals(oldElement) ? element.DataOperationType : OperationType.Update;
//			}

//			foreach (var shift in ShiftsDtos)
//			{
//				foreach (var schedule in shift.schedules)
//				{
//					var sched = GridDataSchedules.FirstOrDefault(x => x.Id == schedule.id);
//					if (sched != null)
//					{
//						sched.CustomInitHour = (schedule.initHour?.TimeOfDay != shift.initHour) ? schedule.initHour?.TimeOfDay.TotalHours : null;
//						sched.CustomEndHour = (schedule.endHour?.TimeOfDay != shift.endHour) ? schedule.endHour?.TimeOfDay.TotalHours : null;

//						if (sched.CustomInitHour != null)
//						{
//							if (schedule.initHour?.TimeOfDay != shift.initHour)
//							{
//								sched.DataOperationType = OperationType.Update;
//							}
//						}
//					}
//				}
//			}

//			ShiftDto = new();
//			SelectedData.Clear();
//			validInput = false;
//			IsDirty = IsDirtyModel();
//			HasTimeChanged = false;
//			StateHasChanged();
//			showModalEditSchedules = false;
//			IsDirtyWorkerRoles = false;
//		}

//		private bool Validate()
//		{
//			ValidationErrors.Clear();

//			if (string.IsNullOrEmpty(ShiftDto.name))
//				ValidationErrors.Add("ComboSchedules", MessageMandatoryField);


//			if (!HasTimeChanged || HasBreakChange)
//				return true;


//			return ValidationErrors.Any();
//		}

//		private string GetValidationError(string fieldName)
//		{
//			string errors = ValidationErrors.TryGetValue(fieldName, out var error) ? error : string.Empty;
//			return errors;
//		}

//		private void OnSelectedDataItemsChanged(IReadOnlyList<object> newSelection)
//		{
//			try
//			{
//				foreach (var item in newSelection)
//					if (!SelectedData.Contains(item))
//						SelectedData.Add(item);

//				var itemsToRemove = SelectedData.Except(newSelection).ToList();
//				foreach (var item in itemsToRemove)
//					SelectedData.Remove(item);

//				var cont = SelectedData.Count;
//				Validate();
//			}
//			catch (Exception ex)
//			{
//				throw ex;
//			}
//		}

//		void SelectedShiftChanged(ShiftDto shift)
//		{
//			ShiftDto = shift;
//			copyShifDto = ShiftDto.DeepClone();
//			CustomShiftDto = ShiftDto.DeepClone();
//			copyCustomShifDto = CustomShiftDto.DeepClone();
//			workerScheduleDtos = ShiftDto.schedules;
//			SelectedData = workerScheduleDtos.Where(w => w.shift.Id == shift.id).Cast<object>().ToList();
//			isAnyWorkerSelected = false;
//			StateHasChanged();
//			Validate();
//		}

//		private void HandleTimeChanged(TimeSpan newTime, string propertyName)
//		{
//			HasTimeChanged = false;

//			if (copyShifDto != null)
//			{
//				if (propertyName == nameof(ShiftDto.initHour))
//					ShiftDto.initHour = newTime;
//				else if (propertyName == nameof(ShiftDto.endHour))
//					ShiftDto.endHour = newTime;

//				if (!copyShifDto.Equals(ShiftDto))
//				{
//					HasTimeChanged = true;
//				}
//			}
//		}

//		private void HandleCustomTimeChanged(TimeSpan newTime, string propertyName)
//		{
//			if (copyCustomShifDto != null)
//			{
//				if (propertyName == nameof(CustomShiftDto.initHour))
//					CustomShiftDto.initHour = newTime;
//				else if (propertyName == nameof(CustomShiftDto.endHour))
//					CustomShiftDto.endHour = newTime;
//			}
//		}

//		private void HandleSelectWorkers(List<ShiftSheduleDto> workerSchedules)
//		{
//			if (workerSchedules.Count() > 0)
//			{
//				isAnyWorkerSelected = true;
//				CustomShiftDto.initHour = workerSchedules[^1].initHour?.TimeOfDay ?? new TimeSpan();
//				CustomShiftDto.endHour = workerSchedules[^1].endHour?.TimeOfDay ?? new TimeSpan();

//				StateHasChanged();
//			}
//			else
//			{
//				isAnyWorkerSelected = false;
//				StateHasChanged();
//			}
//			SelectedWorkerSchedules = workerSchedules;
//		}

//		private void UpdateWorkersGrid()
//		{
//			SelectedWorkerSchedules.ForEach(x =>
//			{
//				x.initHour = DateTime.Today.Add(CustomShiftDto.initHour);
//				x.endHour = DateTime.Today.Add(CustomShiftDto.endHour);
//			});

//			var originalSchedules = ShiftsDtos.First(x => x.id == ShiftDto.id).schedules;
//			HasTimeChanged = originalSchedules.Any(x => originalSchedules.Any(y => y.id == x.id && (y.initHour != x.initHour || y.endHour != x.endHour)));

//			SelectedWorkerSchedules = new List<ShiftSheduleDto>();

//			isAnyWorkerSelected = false;

//			// CustomShiftDto.initHour = ShiftsDtos.First(x => x.id == ShiftDto.id).initHour;
//			// CustomShiftDto.endHour = ShiftsDtos.First(x => x.id == ShiftDto.id).endHour;

//			StateHasChanged();

//			IsDirtyWorkerRoles = true;
//		}

//		private void ChangeWorkersStops(List<StopDto> stops)
//		{
//			HasBreakChange = true;
//			AllStops = stops;
//			IsDirtyWorkerRoles = true;
//			StateHasChanged();
//		}

//		private bool IsDirtySchedule() => !IsDirtyWorkerRoles;

//		private void AfterTimeChanged(ShiftDto shift) => ShowHidePanel(false, false);

//		private void OnValueChanged(ResourceDto resource) => ShowHidePanel(false, false);
//		#endregion
//	}
//}
