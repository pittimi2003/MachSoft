using Mecalux.ITSW.ApplicationDictionary.Widgets;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Preview;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.HeaderEnums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Models.Resources;

namespace Mss.WorkForce.Code.Web
{
    public class SimulateService(IHttpClientFactory httpClientFactory, DataAccess dataAccess, ILogger<SimulateService> logger) : ISimulateService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private readonly ILogger<SimulateService> _logger = logger;

        public async Task<GanttDataConvertDto<TaskData>> GetSimulateData(Guid WarehouseId, UserFormatOptions userFormat, EnumViewPlanning View)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
            client.DefaultRequestHeaders.Add("Simulation-Case", SimulationCase.Planning.ToString());
            GanttDataConvertDto<TaskData> planning = new GanttDataConvertDto<TaskData>();
            try
            {
                var response = await client.GetAsync("simulate");
                response.EnsureSuccessStatusCode();
                string planningString = await response.Content.ReadAsStringAsync();
                Guid planningID = JsonConvert.DeserializeObject<Guid>(planningString);
                if (!dataAccess.IsPlanningInWarehouse(planningID, WarehouseId))
                    planningID = Guid.Empty;

			    var simulationData = dataAccess.GetPlannings(planningID);
                var warehouseProcessPlannings = dataAccess.GetWarehouseProcessPlannings(planningID);
                planning = GanttConverter.ConvertToGanttData(simulationData, warehouseProcessPlannings, userFormat, View);
                planning.PlanningId = planningID;
                planning.itemsPlanning = simulationData.ToList();
                planning.warehouseProcessPlanning = warehouseProcessPlannings.ToList();
                planning.PlanningData = GanttConverter.ConvertOrdersToMetricsPlanning(simulationData.Select(m => m.WorkOrderPlanning).DistinctBy(m => m.Id), simulationData, userFormat.TimeZoneOffSet);

            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error HTTP: {httpEx.Message}");
            }
            catch (NotSupportedException notSupportedEx)
            {
                _logger.LogError($"Error de formato: {notSupportedEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            return planning;
        }

        public async Task<PreviewData> GetSimulateDataPreview(Guid WarehouseId,TemporalidadModel Temp, List<VehicleLoadDto> workLoad, List<ShiftRolDto> Shifts, UserFormatOptions userFormat)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
            client.DefaultRequestHeaders.Add("UserFormat", JsonConvert.SerializeObject(userFormat));
            PreviewData planning = new PreviewData();
            try
            {

                var parameters = new PreviewDto()
                {
                  Temporalidad = Temp,
                  Loadinput = workLoad.Where(x => !x.isOut).Select(x => new LoadDto
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
                  }).ToList(),
                    LoadOutput = workLoad.Where(x => x.isOut).Select(x => new LoadDto
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
                    }).ToList(),
                    ShiftRolDto = Shifts
                };

                var response = await client.PostAsJsonAsync<PreviewDto>("simulate", parameters);
                response.EnsureSuccessStatusCode();
                string planningString = await response.Content.ReadAsStringAsync();
                planning = JsonConvert.DeserializeObject<PreviewData>(planningString);

            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error HTTP: {httpEx.Message}");
            }
            catch (NotSupportedException notSupportedEx)
            {
                _logger.LogError($"Error de formato: {notSupportedEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            return planning;
        }

        public async Task<List<SimulationLogCheck>> GetSimulateDataLog(Guid WarehouseId, TemporalidadModel Temp, List<LoadDto> loadInput, List<LoadDto> loadOut, List<ShiftRolDto> Shifts)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
            List<SimulationLogCheck> planning = new List<SimulationLogCheck>();
            try
            {

                var parameters = new PreviewDto()
                {
                    Temporalidad = Temp,
                    Loadinput = loadInput,
                    LoadOutput = loadOut,
                    ShiftRolDto = Shifts
                };

                var response = await client.PostAsJsonAsync<PreviewDto>("simulatePreviewLog", parameters);
                response.EnsureSuccessStatusCode();
                string planningString = await response.Content.ReadAsStringAsync();
                planning = JsonConvert.DeserializeObject<List<SimulationLogCheck>>(planningString);

            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error HTTP: {httpEx.Message}");
            }
            catch (NotSupportedException notSupportedEx)
            {
                _logger.LogError($"Error de formato: {notSupportedEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            return planning;
        }

        public async Task<List<WorkerWhatIf>> GetWhatIfLog(Guid WarehouseId, TemporalidadModel Temp, List<LoadDto> loadInput, List<LoadDto> loadOut, List<ShiftRolDto> Shifts)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
            List<WorkerWhatIf> planning = new List<WorkerWhatIf>();
            try
            {

                var parameters = new PreviewDto()
                {
                    Temporalidad = Temp,
                    Loadinput = loadInput,
                    LoadOutput = loadOut,
                    ShiftRolDto = Shifts
                };

                var response = await client.PostAsJsonAsync<PreviewDto>("whatIf", parameters);
                response.EnsureSuccessStatusCode();
                string planningString = await response.Content.ReadAsStringAsync();
                planning = JsonConvert.DeserializeObject<List<WorkerWhatIf>>(planningString);

            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error HTTP: {httpEx.Message}");
            }
            catch (NotSupportedException notSupportedEx)
            {
                _logger.LogError($"Error de formato: {notSupportedEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            return planning;
        }

        public async Task UpdateModel(string model)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            try
            {
                var modelJson = ModelConverter.ConvertJson(model);
                HttpContent content = new StringContent(modelJson);
                var response = await client.PostAsync("database", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }

        public async Task SaveChangesInDataBase(OperationDB model)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);

            try
            {
                var stringModel = JsonConvert.SerializeObject(model, Formatting.Indented);
                HttpContent content = new StringContent(stringModel);
                var response = await client.PostAsync("database", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                _logger.LogError($"Error");
            }
        }

        public int GetWidgets(eGroupWidgets eGroupWidgets)
        {
            int widgetsCount = 0;
            try
            {
                List<string> widgetsBD = new();
                switch (eGroupWidgets)
                {
                    case eGroupWidgets.Planning:
                        widgetsBD = DataAccess.GetWidgets();
                        break;
                    case eGroupWidgets.Workers:
                        widgetsBD = DataAccess.GetWidgetsWorkers();
                        break;
                }

                XDocument dashboardXml = XDocument.Parse(widgetsBD[0]);
                IWidgetPanelManager panelManager = WidgetPanelManagerFactory.Create();
                var settings = new WidgetPanelDecomposeSettings();
                var pdef = panelManager.Decompose(widgetsBD[0], settings);
                widgetsCount = pdef.Widgets.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            return widgetsCount;

        }

        public async Task SaveDataLockTaskPlanning(List<TaskData> selectTask, Guid WarehouseId, double offset = 0)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());

            List<Object> orderBlock = new List<object>();

            try
            {
                foreach(TaskData task in selectTask)
                {
                    orderBlock.Add(new
                    {
                        Id = task.id,
                        IsBlocked = task.isBlock,
                        BlockDate = task.blockDate == DateTimeOffset.MinValue ? task.blockDate : task.blockDate.AddHours(-offset),
                        Duration = (task.StartDate - task.EndDate).TotalSeconds,
                    });
                }

                var stringModel = JsonConvert.SerializeObject(orderBlock, Formatting.Indented);
                HttpContent content = new StringContent(stringModel);
                var response = await client.PostAsync("action/orderblock", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }

        public async Task CancelTaskPlanning(List<TaskData> selectTask, Guid WarehouseId)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());

            try
            {
                var payload = new List<string>();
                foreach (var x in selectTask)
                    payload.Add(x.id.ToString());
                var stringModel = JsonConvert.SerializeObject(payload);
                var content = new StringContent(stringModel, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("action/ordercancel", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cancelar alguna orden de la lista {selectTask}: {ex.Message}");
                throw;
            }
        }

        public async Task<Guid> CloneLayout(Guid layoutId)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add(HeaderEnums.LayoutId.ToString(), layoutId.ToString());

            try
            {
                var response = await client.GetAsync("layout");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return Guid.TryParse(responseContent, out var newLayoutId) ? newLayoutId : Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al clonar el layout: {ex.Message}");
                return Guid.Empty;
            }
        }

        public async Task<Guid> ExecutePlanningSimulation(Guid warehouseId, SimulationCase simulation = SimulationCase.Planning)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
            client.DefaultRequestHeaders.Add("Simulation-Case", simulation.ToString());

            try
            {
                var response = await client.GetAsync(Constants.SimulateEndpoint);
                response.EnsureSuccessStatusCode();
                Guid planningID = await response.Content.ReadFromJsonAsync<Guid>();

                return planningID;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error HTTP: {httpEx.Message}");
            }
            catch (NotSupportedException notSupportedEx)
            {
                _logger.LogError($"Error de formato: {notSupportedEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }

            return Guid.Empty;
        }

        public GanttDataConvertDto<LaborTaskGantt> GetLaborTasksByPlanningId(Guid planningId, UserFormatOptions userFormat)
        {
            try
            {
                List<Break> breaksCollections = new List<Break>();

                var laborWorkerCollections = dataAccess.GetWFMLaborsByPlanningId(planningId).ToList();

                if (!laborWorkerCollections.Any())
                {
                    _logger.LogWarning($"SimulateService::GetLaborTasksByPlanningId - No labor workers data found for PlanningId: {planningId}");
                    return new GanttDataConvertDto<LaborTaskGantt>();
                }
                else if (laborWorkerCollections.Any() && laborWorkerCollections is List<WFMLaborWorker> wFMLabors)
                {
                    breaksCollections = [.. dataAccess.GetBreakByBreakprofilesId([.. wFMLabors.Select(x => x.Schedule.BreakProfileId)])];
                }

                var laborItemPlanningCollections = dataAccess.GetWFMLaborItemPlanningByPlanningId(planningId);

                var laborTasks = LaborManagementGanttConverter.LaborWorkersTaskConverter(laborWorkerCollections, laborItemPlanningCollections, breaksCollections, userFormat);

                return laborTasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SimulateService::GetLaborTasksByPlanningId - An error occurred while retrieving and converting labor tasks and planning data for PlanningId: {planningId}");
                return new GanttDataConvertDto<LaborTaskGantt>();
            }
        }

        public GanttDataConvertDto<LaborEquipmentGantt> GetLaborEquipmentsByPlanningId(Guid planningId, UserFormatOptions userFormat)
        {
            try
            {
                var laborEquipmentsCollections = dataAccess.GetWFMLaborEquipmentsByPlanningId(planningId).ToList();

                if (!laborEquipmentsCollections.Any())
                {
                    _logger.LogWarning($"SimulateService::GetLaborEquipmentsByPlanningId - No labor equipment data found for PlanningId: {planningId}");
                    return new GanttDataConvertDto<LaborEquipmentGantt>();
                }

                var laborItemPlanningCollections = dataAccess.GetWFMLaborItemPlanningEquipmentByPlanningId(planningId).ToList();


                var laborTasks = LaborManagementGanttConverter.LaborEquipmentsTaskConverter(laborEquipmentsCollections, laborItemPlanningCollections, userFormat);

                return laborTasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SimulateService::GetLaborEquipmentsByPlanningId - An error occurred while retrieving and converting labor equipments tasks and planning data for PlanningId: {planningId}");
                return new GanttDataConvertDto<LaborEquipmentGantt>();
            }
        }

        public GanttDataConvertDto<YardTaskGantt> GetYardsByPlanningId(Guid planningId, UserFormatOptions userFormat)
        {
            try
            {
                var yardsAppointmentCollections = dataAccess.GetYardMetricsAppointmentByPlanningId(planningId).ToList();

                if (!yardsAppointmentCollections.Any())
                {
                    _logger.LogWarning($"SimulateService::GetYardsByPlanningId - No yard data found for PlanningId: {planningId}");
                    return new GanttDataConvertDto<YardTaskGantt>();
                }

                // Obtengo todas las license de las citas de planning,
                var licences = yardsAppointmentCollections.Select(x => x.License).Distinct().ToList();

                // Obtenemos el VehicleCode con las referencia cruzada del campo "Licences" en la tabla "YardAppointmentsNotifications"
                // Por ahora se obtiene el "VehicleCode" de la tabla "YardAppointmentsNotifications", pero debe existir el campo
                // "VehicleCode" en la relación de las tabla "YardMetricsAppointments"
                var trailers = dataAccess.GetVehicleCodeByLicences(licences);

                return YardGanttConverter.YardTaskGanttConvert(yardsAppointmentCollections, trailers, userFormat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"SimulateService::GetYardsByPlanningId - An error occurred while retrieving and converting yard tasks and planning data for PlanningId: {planningId}");
                return new GanttDataConvertDto<YardTaskGantt>();
            }
        }

        public async Task SaveDataChangePriorityPlanning(string newPriority, List<TaskData> ListSelectTask, Guid WarehouseId)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
            try
            {
                ChangePriorityDto changePriorityTasks = new ChangePriorityDto();
                changePriorityTasks.WorkOrderId = new();

                foreach (var taskData in ListSelectTask)
                    changePriorityTasks.WorkOrderId.Add(taskData.id);

                changePriorityTasks.Priority = newPriority;

                var stringModel = JsonConvert.SerializeObject(changePriorityTasks, Formatting.Indented);
                HttpContent content = new StringContent(stringModel);
                var response = await client.PostAsync("action/changepriority", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, List<ResourceMessage>>> CheckConfiguration(Guid warehouseId)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
            try
            {
                using var response = await client.GetAsync("configcheck");
                response.EnsureSuccessStatusCode();

                var res = await response.Content.ReadFromJsonAsync<Dictionary<string, List<ResourceMessage>>>();
                return res ?? new Dictionary<string, List<ResourceMessage>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckConfiguration");
                return new Dictionary<string, List<ResourceMessage>>();
            }
        }

        public async Task SaveDataPreview(TemporalidadModel temporalidadSeleccionada, List<VehicleLoadDto> load, List<ShiftRolDto> shifts)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", temporalidadSeleccionada.WarehouseId.ToString());
            try
            {
                foreach (var item in shifts) 
                {
                    item.IsNew = false;
                    foreach (var roles in item.workersInRol)
                    {
                        roles.IsNew = false;
                    }
                }
                PreviewDto Data = new PreviewDto()
                {
                    Temporalidad = temporalidadSeleccionada,
                    Loadinput = load.Where(x => !x.isOut).Select(x => new LoadDto
                    {
                        DataOperationType = x.DataOperationType,
                        endHour = x.endHour,
                        hour = x.hour,
                        id = x.id,
                        load = x.load,
                        loadId = x.loadId,
                        numberVehicle = x.numberVehicle,
                        vehicle = x.vehicle,
                        vehicleId = x.vehicleId,
                        ModifiedFields = x.ModifiedFields,

                    }).ToList(),
                    LoadOutput = load.Where(x => x.isOut).Select(x => new LoadDto
                    {
                        DataOperationType = x.DataOperationType,
                        endHour = x.endHour,
                        hour = x.hour,
                        id = x.id,
                        load = x.load,
                        loadId = x.loadId,
                        numberVehicle = x.numberVehicle,
                        vehicle = x.vehicle,
                        vehicleId = x.vehicleId,
                        ModifiedFields = x.ModifiedFields,
                    }).ToList(),
                    ShiftRolDto = shifts
                };
                var stringModel = JsonConvert.SerializeObject(Data, Formatting.Indented);
                HttpContent content = new StringContent(stringModel);
                var response = await client.PostAsync("savepreview", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                _logger.LogError($"Error");
            }
        }

        public class PreviewDto
        {
            public TemporalidadModel Temporalidad { get; set; }
            public List<LoadDto> Loadinput { get; set; }
            public List<LoadDto> LoadOutput { get; set; }
            public List<ShiftRolDto> ShiftRolDto { get; set; }
        }
    }
}
