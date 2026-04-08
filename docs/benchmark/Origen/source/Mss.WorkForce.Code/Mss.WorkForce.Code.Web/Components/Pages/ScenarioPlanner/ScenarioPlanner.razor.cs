using DevExpress.Blazor;
using Microsoft.AspNetCore.Components;

using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Preview;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using Mss.WorkForce.Code.Web.Components.Pages.Preview.Components;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Services;
using Newtonsoft.Json;


namespace Mss.WorkForce.Code.Web.Components.Pages.ScenarioPlanner
{
    public partial class ScenarioPlanner : GanttOperations
    {
        private DxGrid gridLoad;

        private Temporalidad temporalityComponent;
        private List<TemporalidadModel> temps = new List<TemporalidadModel>();
        private TemporalidadModel temporalidadSeleccionada { get; set; }
        public SiteModel CurrentSite { get; set; } = new SiteModel();

        private bool IsDirty { get; set; }

        private PreviewDto Data { get; set; }

        public List<LoadDto> LoadInput { get; set; }
        public List<LoadDto> LoadOutput { get; set; }

        public List<VehicleLoadDto> Load { get; set; }
        public List<VehicleLoadDto> LoadBeforeEdit { get; set; }

        public UserFormatOptions userFormat { get; set; } = new();

        private TemporalidadModel defaultTemp;

        private List<VehicleMetricsDto> VehicleMetrics;

        private List<WorkerWhatIf> WorkersDistribution;

        // Lista que enlazas al grid
        private List<ShiftRolDto> Shifts = new();
        private List<ShiftRolDto> ShiftsBeforeEdit = new();

        // Lista para modal what if
        private List<ShiftRolDto> ShiftsWhatIf = new();

        private List<ShiftRolDto> OriginalWorkers = new();
        private List<LoadDto> OriginalLoadInput = new();
        private List<LoadDto> OriginalLoadOutput = new();

        public GanttDataConvertDto<TaskData> DataSimulation = new();
        public GetAttributesDto<TaskData>? GridParameters { get; set; }


        private List<ShiftRolDto> SelectedShift = new();
        public List<VehicleLoadDto> SelectedLoad = new();
        private bool HasAnySelection =>
              SelectedShift.Any() ||
              SelectedLoad.Any();

        // Opcional: aquí expones el DTO actualizado al exterior
        public ShiftRolDto LastUpdatedShift { get; private set; }
        public WorkersInRolDto LastUpdatedRole { get; private set; }

        IEnumerable<LoadType> loadTypes = new List<LoadType>();
        IEnumerable<VehicleType> vehicleTypes = new List<VehicleType>();

        private bool isSaving { get; set; }


        private string CaptionSimulation = "Simulate";

        private string Inbound_Text = "Inbound";
        private string Outbound_Text = "Outbound";

        private string Schedule = "Schedule_preview";
        private string Name = "Name";
        private string Shift_start = "Shift start";
        private string Shift_end = "Shift end";
        private string Role = "Role";
        private string Operators = "Operators";
        private string Workload = "Workload";
        private string Delete_selected = "Delete selected";
        private string Create_new = "Create new";
        private string Flow = "Flow";
        private string Load_Text = "Load";
        private string Select = "Select...";
        private string Vehicle = "Vehicle";
        private string N_Vehicles = "N° Vehicles";
        private string Start = "Start";
        private string End = "End";
        private string Status = "Status";
        private string Theoretical_departure = "Theoretical departure";
        private string Theoretical_dwell = "Theoretical dwell";
        private string Departure = "Departure";
        private string OTIF = "OTIF";
        private string Delay = "Delay";
        private string Cancel = "Cancel";
        private string Save = "Save";
        private string Slack = "Slack";
        private string Delay_Text = "Delay";
        private string WhatIfTitleToolTip = "WHATIF_TITLE_TOOLTIP";
   

        private string INBOUND = @"<span class=""tag-inbound""><i class=""mlxPanel mlx-ico-entrada""></i> Inbound</span>";
        private string OUTBOUND = @"<span class=""tag-outbound""><i class=""mlxPanel mlx-ico-salida""></i> Outbound</span>";

        private List<FlowTypeDto> TypeBounds = new();

        private bool ShowGroupPanelLoad { get; set; } = true;

        private bool _collapseDetailRowsPending;

        private bool ShowWhatIfModal = false;
        private bool ShowWhatIDistributionfModal = false;
        private List<WorkerWhatIf>? WhatIfLogs;

        private async Task LoadWhatIf()
        {
            ShowWhatIfModal = false; 

            var Loadinput = Load.Where(x => !x.isOut).Select(x => new LoadDto
            {
                DataOperationType = x.DataOperationType,
                endHour = x.endHour,
                hour = x.hour,
                id = x.id,
                load = x.load,
                loadId = x.loadId,
                numberVehicle = x.numberVehicle,
                vehicle = x.vehicle,
                vehicleId = x.vehicleId
            }).ToList();
            var LoadOutput = Load.Where(x => x.isOut).Select(x => new LoadDto
            {
                DataOperationType = x.DataOperationType,
                endHour = x.endHour,
                hour = x.hour,
                id = x.id,
                load = x.load,
                loadId = x.loadId,
                numberVehicle = x.numberVehicle,
                vehicle = x.vehicle,
                vehicleId = x.vehicleId
            }).ToList();

            WhatIfLogs = await _SimulateService.GetWhatIfLog(
                GanttSetup.WarehouseId,
                temporalidadSeleccionada,
                Loadinput,
                LoadOutput,
                Shifts
            );

            if (WhatIfLogs == null || Shifts == null)
                return;

            AnalyzeOperationalCapacity(WhatIfLogs, Shifts);


            ShowWhatIfModal = true;

            ShowHidePanel(false, false);
        }

        private void OpenWhatIfDistributionModal()
        {
            ShowWhatIDistributionfModal = true;
            ShowHidePanel(false, false);
        }

        public void ApplyWhatIf(bool opened)
        {
            if (WhatIfLogs == null)
                return;


            foreach (var shift in Shifts)
            {
                shift.workersInRol?.Clear();
            }


            foreach (var custWorker in WhatIfLogs)
            {
                var shift = Shifts.FirstOrDefault(x => x.id == custWorker.ShiftId);
                if (shift == null)
                    continue;

                shift.workersInRol ??= new List<WorkersInRolDto>();

                var rolWorker = shift.workersInRol
                    .FirstOrDefault(x => x.id == custWorker.RolId);

                if (rolWorker != null)
                {
                    rolWorker.Workers += 1;
                }
                else
                {
                    shift.workersInRol.Add(new WorkersInRolDto
                    {
                        id = custWorker.RolId,
                        Workers = 1,
                        IsNew = true,
                        name = custWorker.Rol
                    });
                }
            }

            IsDirty = true;
            ShowWhatIfModal = opened;
            ShowHidePanel(false, false);
        }

        private void OnWhatIfModalChanged(bool opened)
        {
            ShowWhatIfModal = opened;
            ShowHidePanel(false, false);
        }

        private void OnWhatIfDistributionModalChanged(bool opened)
        {
            ShowWhatIDistributionfModal = opened;
            ShowHidePanel(false, false);
        }

        public override async Task LoadDataSimulation()
        {
            SeDataSimulation();
            var parametrizedDto = GanttSetup.value as SimulationParametrizedDto;
            var previewData = await _SimulateService.GetSimulateDataPreview(GanttSetup.WarehouseId, temporalidadSeleccionada, Load, Shifts, userFormat);

            DataSimulation = previewData.GanttData;
            VehicleMetrics = previewData.VehicleMetrics;
            WorkersDistribution = previewData.WorkersDistribution;

            if (DataSimulation == null || VehicleMetrics == null)
            {
                ShowHidePanel(false, false);
                return;
            }

            ConfigureErrors();

            //await PreparedData();
            await base.LoadDataSimulation();

            await InvokeAsync(StateHasChanged);
        }



        private async Task OnTemporalidadSeleccionadaChanged(TemporalidadModel newTemporalidad)
        {
            ShowHidePanel(false, false);

            CollapseDetailRowsWithoutErrors();

            temporalidadSeleccionada = newTemporalidad;

            if (isSaving)
            {
                await _SimulateService.SaveDataPreview(temporalidadSeleccionada, Load, Shifts);
                ReloadTemporalityData(CurrentSite.Id);
                temporalidadSeleccionada.Data = temps.FirstOrDefault(x => x.Id == temporalidadSeleccionada.Id)?.Data;
                isSaving = false;
            }

            if (temporalidadSeleccionada.Nombre == "DEFAULT") UpdateBaseData();
            else{
                UpdateConfigs();
            }
            

            ShowHidePanel(true, true);
            GanttSetup.ChooserColumnDefault = false;
            await LoadDataSimulation();
        }

        public override Task OnSiteSeleccionadaChanged((SiteModel newSite, bool changeColumns) args)
        {
            CollapseDetailRowsWithoutErrors();
            CurrentSite = args.newSite;
            ReloadTemporalityData(CurrentSite.Id);
            temporalidadSeleccionada = temps.FirstOrDefault();
            LoadData();
            ReloadOriginalList(CurrentSite.Id);
            UpdateBaseData();
            return base.OnSiteSeleccionadaChanged(args);
        }

        private void UpdateBaseData()
        {
            Shifts = OriginalWorkers.Select(x => new ShiftRolDto
            {
                endHour = x.endHour,
                id = x.id,
                initHour = x.initHour,
                IsNew = x.IsNew,
                ModifiedFields = new(),
                name = x.name,
                workersInRol = x.workersInRol.Select(y => new WorkersInRolDto
                {
                    id = y.id,
                    name = y.name,
                    Workers = y.Workers,
                }).ToList(),
            }).ToList();

            Load = OriginalLoadInput.Select(x => new VehicleLoadDto
            {
                DataOperationType = x.DataOperationType,
                endHour = x.endHour,
                hour = x.hour,
                id = x.id,
                isOut = false,
                load = x.load,
                loadId = x.loadId,
                numberVehicle = x.numberVehicle,
                vehicle = x.vehicle,
                vehicleId = x.vehicleId
            }).Concat(OriginalLoadOutput.Select(x => new VehicleLoadDto
            {
                DataOperationType = x.DataOperationType,
                endHour = x.endHour,
                hour = x.hour,
                id = x.id,
                isOut = true,
                load = x.load,
                loadId = x.loadId,
                numberVehicle = x.numberVehicle,
                vehicle = x.vehicle,
                vehicleId = x.vehicleId
            })).ToList();

            SaveSnapshot();
        }

        private async void SaveDataPreview()
        {
            isSaving = true;
            if (temporalidadSeleccionada.Nombre == "DEFAULT")
            {
                ShowHidePanel(false, false);
                temporalityComponent.AbrirModalCrear();
                ShowHidePanel(false, false);
                return;
            }
            try
            {
                await _SimulateService.SaveDataPreview(temporalidadSeleccionada, Load, Shifts);
                SaveTemporalityDate();
                ReloadTemporalityData(CurrentSite.Id);
                isSaving = false;
                IsDirty = false;
                ShowHidePanel(false, false);
                ResetModifiedFields();
                SaveSnapshot();
                InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                isSaving = false;
            }

        }

        private void SaveTemporalityDate()
        {
            var fechaDesde = _DataAccess.GetSequenceByConfigurationSequence(temporalidadSeleccionada.Id).FirstOrDefault();
            var fechaHasta = _DataAccess.GetSequenceByConfigurationSequence(temporalidadSeleccionada.Id).LastOrDefault();
            int totalDays = (temporalidadSeleccionada.FechaHasta - temporalidadSeleccionada.FechaDesde).Days;

            var sequences = _DataAccess.GetSequenceByConfigurationSequence(temporalidadSeleccionada.Id).ToList();

            if (fechaHasta.Date.Day < temporalidadSeleccionada.FechaHasta.Day && fechaDesde.Date.Day == temporalidadSeleccionada.FechaDesde.Day)
                AddConfigurationSequence(totalDays);
            else
            {
                AddConfigurationSequence(totalDays);
                DeleteConfigurationSequence(fechaHasta.Sequence, sequences);
            }
        }

        private void AddConfigurationSequence(int totalDays)
        {
            for (int i = 0; i <= totalDays; i++)
            {
                var newSequence = new ConfigurationSequence
                {
                    Id = Guid.NewGuid(),
                    Date = temporalidadSeleccionada.FechaDesde.AddDays(i),
                    Sequence = 0,
                    ConfigurationSequenceHeaderId = temporalidadSeleccionada.Id,
                    ConfigurationSequenceHeader = null,
                };
                _DataAccess.AddConfigurationSequence(newSequence);
            }
        }

        private void DeleteConfigurationSequence(int totalDays, IEnumerable<ConfigurationSequence> configurationSequences)
        {
            foreach (var item in configurationSequences)
                _DataAccess.RemoveConfigurationSequence(item.Id);
        }

        private async void CancelDataPreview()
        {
            Undo();
            IsDirty = false;
        }

        private void ReloadTemporalityData(Guid siteId)
        {
            temps.Clear();
            temps.Add(defaultTemp);
            temps.AddRange(_DataAccess.GetConfigurationSequencesByWarehouse(siteId)
            .Select(x => new TemporalidadModel(userFormat.DateFormat) { Id = x.Id, Nombre = x.Code, Data = x.Data, WarehouseId = x.WarehouseId, FechaDesde = x.ConfigurationSequences.Min(m => m.Date).Date, FechaHasta = x.ConfigurationSequences.Max(m => m.Date).Date })
            .ToList());
        }

        public override void LoadGanttSetupData(EnumViewPlanning newView)
        {
            GanttSetup.WarehouseId = CurrentSite.Id;
            GanttSetup.typeGantt = Models.SignalR.GanttView.Preview;
            GanttSetup.EditGantt = true;
            GanttSetup.ShowToltip = false;
            SeDataSimulation();
        }

        private void SeDataSimulation()
        {
            GanttSetup.value = Data;
        }

        public async Task GetUserDataAsync()
        {
            await _InitialDataService.GetDataUserLocal();
            userFormat = _InitialDataService.GetUserFormat();
        }

        public void Translate()
        {
            Inbound_Text = L.Loc("Inbound");
            Outbound_Text = l.Loc("Outbound");
            Schedule = l.Loc("Schedule_preview");
            Name = l.Loc("Shift");
            Shift_start = l.Loc("Start");
            Shift_end = l.Loc("End");
            Role = l.Loc("Role");
            Operators = l.Loc("Operators");
            Workload = l.Loc("Workload");
            Delete_selected = l.Loc("Delete selected");
            Create_new = l.Loc("Create new");
            Flow = l.Loc("Flow");
            Load_Text = l.Loc("Load");
            Select = l.Loc("Select...");
            Vehicle = l.Loc("Vehicle");
            N_Vehicles = l.Loc("N° Vehicles");
            Start = l.Loc("Start");
            End = l.Loc("End");
            Status = l.Loc("Status");
            Theoretical_departure = l.Loc("Theoretical departure");
            Theoretical_dwell = l.Loc("Theoretical dwell");
            Departure = l.Loc("Departure");
            OTIF = l.Loc("OTIF");
            Delay = l.Loc("Delay");
            Cancel = l.Loc("Cancel");
            Save = l.Loc("Save");
            Slack = l.Loc("Slack");
            Delay_Text = l.Loc("Delay");
            WhatIfTitleToolTip = l.Loc(WhatIfTitleToolTip);
        }

        // Carga inicial (ejemplo)
        protected override async Task OnInitializedAsync()
        {
            await GetUserDataAsync();
            Translate();

            INBOUND = $@"<span class=""tag-inbound""><i class=""mlxPanel mlx-ico-entrada""></i> {Inbound_Text}</span>";
            OUTBOUND = $@"<span class=""tag-outbound""><i class=""mlxPanel mlx-ico-salida""></i> {Outbound_Text}</span>";

            ShowHidePanel(true, true);
            GridParameters = new(l);
            ShowInfoDrawer = true;
            base.OnInitialized();
            ShowHidePanel(true, true);

            GanttProperties = GridParameters.GetTranslateCaptions();
            TypeBounds = new List<FlowTypeDto> {
                    new FlowTypeDto { Key = false, Text = Inbound_Text, Html = (MarkupString)INBOUND },
                    new FlowTypeDto { Key = true,  Text = Outbound_Text, Html = (MarkupString)OUTBOUND }
                };


            Sites = _DataAccess.GetWarehouse(_DataAccess.GetOrganization().FirstOrDefault().Id)
                      .Select(x => new SiteModel
                      {
                          Id = x.Id,
                          Name = x.Name
                      }).ToList();

            CurrentSite = Sites.FirstOrDefault();

            LoadData();
            LoadGanttSetupData(EnumViewPlanning.None);

            userFormat = new UserFormatOptions
            {
                HourFormat = "HH:mm",
            };

            defaultTemp = new TemporalidadModel(userFormat.DateFormat) { Nombre = "DEFAULT", Data = "{}" };
            temporalidadSeleccionada = defaultTemp;

            ReloadTemporalityData(CurrentSite.Id);

            ReloadOriginalList(CurrentSite.Id);

            UpdateBaseData();


            //UpdateConfigs();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender || RepaintDataGantt)
            {
                await LoadGanttServiceAsync();
                GanttSetup.ChooserColumnDefault = true;
                GanttSetup.value = Data;
            }

            await RepaintGanttAsync(false, ShowInfoDrawer);
        }

        private void LoadData()
        {
            loadTypes = ProcessLoadService.ProcessLoadTypes(_DataAccess.GetLoadProfiles(CurrentSite.Id));
            vehicleTypes = ProcessLoadService.ProcessVehicleTypes(_DataAccess.GetVehicleProfiles(CurrentSite.Id));
        }

        private void UpdateConfigs()
        {
            var changes = JsonConvert.DeserializeObject<PreviewDto>(temporalidadSeleccionada.Data);

            Data = changes;

            Shifts = Data.ShiftRolDto;
            LoadInput = Data.Loadinput;
            LoadOutput = Data.LoadOutput;

            Load = Data.Loadinput.Select(x => new VehicleLoadDto
            {
                DataOperationType = x.DataOperationType,
                endHour = x.endHour,
                hour = x.hour,
                id = x.id,
                isOut = false,
                load = x.load,
                loadId = x.loadId,
                numberVehicle = x.numberVehicle,
                vehicle = x.vehicle,
                vehicleId = x.vehicleId,
                ModifiedFields = x.ModifiedFields

            }).Concat(Data.LoadOutput.Select(x => new VehicleLoadDto
            {
                DataOperationType = x.DataOperationType,
                endHour = x.endHour,
                hour = x.hour,
                id = x.id,
                isOut = true,
                load = x.load,
                loadId = x.loadId,
                numberVehicle = x.numberVehicle,
                vehicle = x.vehicle,
                vehicleId = x.vehicleId,
                ModifiedFields = x.ModifiedFields
            })).ToList();

            // Se elimina para evitar el pintado de datos editados
            ResetModifiedFields();

            // Creamos una copia original antes de modificar 
            SaveSnapshot();
            IsDirty = false;
        }

        private void ReloadOriginalList(Guid siteId)
        {
            OriginalWorkers = _DataAccess.GetWorkersByShiftAndRol(siteId);
            OriginalLoadInput = ProcessLoadService.Process(_DataAccess.GetInboundLoad(siteId, GetCurrentConfig()), false);
            OriginalLoadOutput = ProcessLoadService.Process(_DataAccess.GetOutboundLoad(siteId, GetCurrentConfig()), true);
        }

        private Guid GetCurrentConfig()
        {
            try
            {
                Guid configGui = Guid.Empty;
                if (temporalidadSeleccionada is TemporalidadModel temporalidad && temporalidad.Nombre != "DEFAULT")
                    configGui = temporalidad.Id;

                return configGui;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        // ---- TURNOS (ShiftRolDto) ----
        void OnShiftEditModelSaving(GridEditModelSavingEventArgs e)
        {
            var edited = (ShiftRolDto)e.EditModel;
            var original = (ShiftRolDto)e.DataItem;
            var dto = original;

            foreach (var prop in typeof(ShiftRolDto).GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                var oldVal = prop.GetValue(original);
                var newVal = prop.GetValue(edited);

                if (!Equals(oldVal, newVal))
                {
                    dto.ModifiedFields.Add(prop.Name);
                }
            }

            e.CopyChangesToDataItem();
            IsDirty = true;

            LastUpdatedShift = dto;
            OnShiftUpdated(dto);
            ShowHidePanel(false, false);
            StateHasChanged();
        }


        // ---- ROLES (WorkersInRolDto) ----
        void OnWorkerEditModelSaving(GridEditModelSavingEventArgs e, ShiftRolDto parentShift)
        {
            var edited = (WorkersInRolDto)e.EditModel;
            var original = (WorkersInRolDto)e.DataItem;
            var dto = original;

            foreach (var prop in typeof(WorkersInRolDto).GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                var oldVal = prop.GetValue(original);
                var newVal = prop.GetValue(edited);

                if (!Equals(oldVal, newVal))
                {
                    dto.ModifiedFields.Add(prop.Name);
                }
            }

            e.CopyChangesToDataItem();
            IsDirty = true;

            LastUpdatedRole = dto;
            OnShiftUpdated(parentShift);
            ShowHidePanel(false, false);
            StateHasChanged();
        }


        // Tu lógica para tratar el DTO
        void OnShiftUpdated(ShiftRolDto dtoActualizado)
        {
            // Aquí tienes el DTO ya actualizado en memoria
            // por ejemplo:
            // IsDirty = true;
            // o lanzar un EventCallback al padre, etc.
        }

        Task<List<ShiftRolDto>> LoadScheduleAsync()
        {
            // Sustituye por tu llamada real
            return Task.FromResult(new List<ShiftRolDto>());
        }

        void OnLoadEditModelSaving(GridEditModelSavingEventArgs e)
        {
            // 1) Comparar editado vs original para rellenar ModifiedFields (tu lógica actual)
            var edited = (VehicleLoadDto)e.EditModel;   // valores nuevos
            var original = (VehicleLoadDto)e.DataItem;    // valores originales
            var dto = original;                      // objeto real en la lista

            foreach (var prop in typeof(VehicleLoadDto).GetProperties())
            {
                if (!prop.CanRead)
                    continue;

                var oldVal = prop.GetValue(original);
                var newVal = prop.GetValue(edited);

                if (!Equals(oldVal, newVal))
                {
                    dto.ModifiedFields.Add(prop.Name);
                }
            }

            // 2) Aplicar los cambios al DataItem
            e.CopyChangesToDataItem();

            // 3) SINCRONIZAR loadId a partir de load (string)
            if (!string.IsNullOrEmpty(dto.load))
            {
                var loadType = loadTypes.FirstOrDefault(x => x.Name == dto.load);
                if (loadType != null)
                {
                    dto.loadId = loadType.Id;
                    dto.ModifiedFields.Add(nameof(dto.loadId));
                }
                else
                {
                    dto.loadId = Guid.Empty;
                }
            }
            else
            {
                dto.loadId = Guid.Empty;
            }

            // 4) SINCRONIZAR vehicleId a partir de vehicle (string)
            if (!string.IsNullOrEmpty(dto.vehicle))
            {
                var vehicleType = vehicleTypes.FirstOrDefault(x => x.Name == dto.vehicle);
                if (vehicleType != null)
                {
                    dto.vehicleId = vehicleType.Id;
                    dto.ModifiedFields.Add(nameof(dto.vehicleId));
                }
                else
                {
                    dto.vehicleId = Guid.Empty;
                }
            }
            else
            {
                dto.vehicleId = Guid.Empty;
            }

            // 5) Marcar sucio y cerrar panel
            IsDirty = true;
            ShowHidePanel(false, false);
        }



        private void OnSelectedLoadChanged(IReadOnlyList<object> newSel)
        {
            SelectedLoad.Clear();
            if (newSel is not null)
            {
                foreach (var o in newSel)
                    if (o is VehicleLoadDto pr) SelectedLoad.Add(pr);
            }
            ShowHidePanel(false, false);
        }

        private void OnSelectedShiftChanged(IReadOnlyList<object> newSel)
        {
            SelectedShift.Clear();
            if (newSel is not null)
            {
                foreach (var o in newSel)
                    if (o is ShiftRolDto pr) SelectedShift.Add(pr);
            }
            ShowHidePanel(false, false);
        }

        private void ConfigureErrors()
        {
            foreach (var slot in Load)
            {
                slot.Errors = VehicleMetrics
                    .Where(a => a.OTIF < 1 && a.orderSheduleId == slot.id)
                    .ToList();

                slot.AllMetrics = VehicleMetrics.Where(a => a.orderSheduleId == slot.id)
                    .ToList();
            }
        }

        private void OnCustomizeGridElement(GridCustomizeElementEventArgs e)
        {
            // ---------- FILAS ----------
            if (e.ElementType == GridElementType.DataRow)
            {
                var rowObj = e.Grid.GetDataItem(e.VisibleIndex);

                // Tu lógica original (errores -> rojo)
                if (rowObj is VehicleLoadDto vRow)
                {


                    // Fila nueva -> naranja fuerte
                    if (vRow.IsNew)
                    {
                        e.CssClass += " row-new";
                    }
                }

                // Shift nueva
                if (rowObj is ShiftRolDto sRow && sRow.IsNew)
                {
                    e.CssClass += " row-new";
                }

                // Worker nuevo
                if (rowObj is WorkersInRolDto wRow && wRow.IsNew)
                {
                    e.CssClass += " row-new";
                }
            }

            // ---------- CELDAS ----------
            if (e.ElementType == GridElementType.DataCell)
            {
                var rowObj = e.Grid.GetDataItem(e.VisibleIndex);

                // MUY IMPORTANTE → DevExpress usa Column.Name, no FieldName
                var fieldName = e.Column?.Name;

                if (string.IsNullOrEmpty(fieldName))
                    return;

                if (rowObj is VehicleLoadDto vRow && fieldName == "metrics")
                {
                    if (vRow.Errors != null && vRow.Errors.Count > 0)
                    {
                        e.CssClass += " mlx-ico-x-preview mlx-chart";
                    }
                    else
                    {
                        e.CssClass += " mlx-ico-check-preview mlx-chart";
                    }
                }

                // LoadDto celda modificada
                if (rowObj is VehicleLoadDto v && v.ModifiedFields.Contains(fieldName))
                {
                    e.CssClass += " modified-cell";
                }

                // ShiftDto celda modificada
                if (rowObj is ShiftRolDto s && s.ModifiedFields.Contains(fieldName))
                {
                    e.CssClass += " modified-cell";
                }

                // WorkerDto celda modificada
                if (rowObj is WorkersInRolDto w && w.ModifiedFields.Contains(fieldName))
                {
                    e.CssClass += " modified-cell";
                }
            }
        }

        private async Task OnLoadChanged(VehicleLoadDto row, string newValue)
        {
            row.load = newValue;
            await gridLoad.SaveChangesAsync();
        }

        private async Task OnVehicleChanged(VehicleLoadDto row, string newValue)
        {
            row.vehicle = newValue;
            await gridLoad.SaveChangesAsync();
        }



        private async Task CreateNewLoad()
        {
            var newItem = new VehicleLoadDto
            {
                id = Guid.NewGuid(),
                isOut = false,
                load = null,
                vehicle = null,
                numberVehicle = 0,
                hour = TimeSpan.Zero,
                endHour = TimeSpan.Zero,
                DataOperationType = OperationType.Insert,
                IsNew = true
            };

            Load.Insert(0, newItem);

            SelectedLoad.Clear();
            SelectedLoad.Add(newItem);



            await InvokeAsync(async () =>
            {
                gridLoad.Reload();
            });

            ShowHidePanel(false, false);
        }


        private async Task DeleteSelectedLoads()
        {
            if (!SelectedLoad.Any())
                return;

            // IDs reales de las filas seleccionadas
            var idsToDelete = SelectedLoad.Select(x => x.id).ToHashSet();

            // Eliminar todos los que coincidan
            Load.RemoveAll(x => idsToDelete.Contains(x.id));

            // Limpiar selección
            SelectedLoad.Clear();

            // Marcar como Dirty si hace falta
            IsDirty = true;

            await InvokeAsync(async () =>
            {
                gridLoad.Reload();
            });

            ShowHidePanel(false, false);

            //await InvokeAsync(StateHasChanged);
        }

        private async Task OnTemporalidadSeleccionadaRemoved(TemporalidadModel newTemporalidad)
        {

            _DataAccess.RemoveTemporality(newTemporalidad.Id);
            ReloadTemporalityData(CurrentSite.Id);
            temporalidadSeleccionada = temps.First(x => x.Nombre == "DEFAULT");
            UpdateBaseData();
            ShowHidePanel(false, false);
        }

        private void CollapseDetailRowsWithoutErrors()
        {
            if (gridLoad == null)
                return;

            int count = gridLoad.GetVisibleRowCount();

            for (int i = 0; i < count; i++)
            {
                if (gridLoad.IsGroupRow(i))
                    continue;

                if (gridLoad.IsDetailRowExpanded(i))
                {
                    gridLoad.CollapseDetailRow(i);
                }
            }
        }

        private void AnalyzeOperationalCapacity(List<WorkerWhatIf> workers, List<ShiftRolDto> currentShifts)
        {
            ShiftsWhatIf = new();

            var shiftsIndex = currentShifts.Select(s => s.DeepClone()).ToDictionary(s => s.id);

            // Resetear valores de worker requeridos.
            foreach (var shift in shiftsIndex.Values)
            {
                shift.workersInRol ??= new List<WorkersInRolDto>();

                foreach (var role in shift.workersInRol)
                    role.RequiredWorkers = 0;
            }

            foreach (var reqWorker in workers)
            {
                // Si no existe el turno lo creamos
                if (!shiftsIndex.TryGetValue(reqWorker.ShiftId, out var shift))
                {
                    shift = new ShiftRolDto
                    {
                        id = reqWorker.ShiftId,
                        name = reqWorker.Shift,
                        initHour = reqWorker.Init?.TimeOfDay ?? TimeSpan.Zero,
                        endHour = reqWorker.End?.TimeOfDay ?? TimeSpan.Zero,
                        IsNew = true,
                        workersInRol = new List<WorkersInRolDto>
                         {
                             {
                                new WorkersInRolDto {
                                 id = reqWorker.RolId,
                                 name = reqWorker.Rol,
                                 IsNew = true,
                                 Workers = 0,
                                 RequiredWorkers = 1 
                                }
                             }
                         }
                    };

                    shiftsIndex.Add(reqWorker.ShiftId, shift);
                }

                var role = shift.workersInRol.FirstOrDefault(r => r.id == reqWorker.RolId);

                // Si no existe el rol en el turno, lo creamos
                if (role == null)
                {
                    shift.workersInRol.Add(new WorkersInRolDto
                    {
                        id = reqWorker.RolId,
                        name = reqWorker.Rol,
                        IsNew = true,
                        Workers = 0,
                        RequiredWorkers = 1
                    });
                }
                else
                    role.RequiredWorkers += 1;
            }

            // Añadimos los shift a lista de Shifts what if
            ShiftsWhatIf.AddRange(shiftsIndex.Values);
        }
    
        private void ResetModifiedFields()
        {
            Shifts?.ForEach(x =>
            {
                if (x == null) return;

                x.ModifiedFields = new();

                x.workersInRol?.ForEach(a =>
                {
                    if (a == null) return;
                    a.ModifiedFields = new();
                });
            });
            Load?.ForEach(x => { if (x == null) return; x.ModifiedFields = new(); });
        }

        private void SaveSnapshot()
        {
            if (Shifts != null)
                ShiftsBeforeEdit = Shifts
                    .Where(s => s != null)
                    .Select(s => s.DeepClone())
                    .ToList();
            if (Load != null)
                LoadBeforeEdit = Load
                    .Where(l => l != null)
                    .Select(l => l.DeepClone())
                    .ToList();
        }

        private void Undo()
        {
            if (ShiftsBeforeEdit != null)
                Shifts = ShiftsBeforeEdit
                    .Where(s => s != null)
                    .Select(s => s.DeepClone())
                    .ToList();

            if (LoadBeforeEdit != null)
                Load = LoadBeforeEdit
                    .Where(l => l != null)
                    .Select(l => l.DeepClone())
                    .ToList();

            ResetModifiedFields();
        }
    }

    public class ComboGeneric<T>
    {
        public T Value { get; set; }
        public string Description { get; set; }
    }

    public class FlowTypeDto
    {
        public bool Key { get; set; }
        public string Text { get; set; }
        public MarkupString Html { get; set; }
    }

}